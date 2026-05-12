using AffaliteBL.DTOs.Auth;
using AffaliteBL.DTOs.NotificationDTOs;
using AffaliteBL.IServices;
using AffaliteDAL.Entities;
using AffaliteDAL.Entities.Constants;
using AffaliteDAL.Entities.Enums;
using AffaliteDAL.IRepo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AffaliteBL.Services;

public class AuthServices : IAuthServices
{
    private static readonly string AffiliateRole = Roles.Affiliate;
    private static readonly string MerchantRole = Roles.Merchant;

    private static readonly HashSet<string> ValidRoles = Roles.All
        .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private readonly IGenericRepository<Merchant> _merchantRepo;
    private readonly IGenericRepository<Affiliate> _affiliateRepo;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtServices _jwtServices;
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthServices(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtServices jwtServices,
        IGenericRepository<Merchant> merchantRepo,
        IGenericRepository<Affiliate> affiliateRepo,
        INotificationService notificationService,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtServices = jwtServices;
        _merchantRepo = merchantRepo;
        _affiliateRepo = affiliateRepo;
        _notificationService = notificationService;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public Task<AuthResponseDTO> RegisterAffiliateAsync(RegisterDTO model)
    {
        return RegisterWithRoleAsync(model, AffiliateRole);
    }

    public Task<AuthResponseDTO> RegisterMerchantAsync(RegisterDTO model)
    {
        return RegisterWithRoleAsync(model, MerchantRole);
    }

    private async Task<AuthResponseDTO> RegisterWithRoleAsync(RegisterDTO model, string role)
    {
        if (await _userManager.FindByEmailAsync(model.Email) is not null)
        {
            return new AuthResponseDTO { Message = "Email is already registered" };
        }

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            return new AuthResponseDTO
            {
                Message = string.Join(", ", createResult.Errors.Select(e => e.Description))
            };
        }

        try
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                return new AuthResponseDTO
                {
                    Message = string.Join(", ", roleResult.Errors.Select(e => e.Description))
                };
            }

            if (role.Equals(MerchantRole, StringComparison.OrdinalIgnoreCase))
            {
                await _merchantRepo.AddAsync(new Merchant { AppUserId = user.Id, Balance = 0 });
            }
            else
            {
                await _affiliateRepo.AddAsync(new Affiliate { AppUserId = user.Id, Balance = 0 });
            }
            await _unitOfWork.SaveChangesAsync();

            var refreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(user);

            var roleName = role.Equals(MerchantRole, StringComparison.OrdinalIgnoreCase) ? "Merchant" : "Affiliate";

            _notificationService.CreateNotification(new CreateNotificationDTO
            {
                UserId = user.Id,
                Title = "Welcome to Affalite!",
                Message = $"Congratulations {model.FullName}! Your {roleName} account has been created successfully.",
                Type = role.Equals("Merchant", StringComparison.OrdinalIgnoreCase)
                    ? NotificationType.Merchant
                    : NotificationType.Affiliate
            });

            await _emailService.SendWelcomeEmailAsync(model.Email, model.FullName, roleName);

            var tokenResult = await _jwtServices.GenerateTokenAsync(user);
            return BuildAuthResponse(user, tokenResult, refreshToken);
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }
    }

    public async Task<AuthResponseDTO> LoginAsync(LoginDTO model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            return new AuthResponseDTO { Message = "Email or password is incorrect" };
        }

        var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
        if (activeRefreshToken is null)
        {
            activeRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(activeRefreshToken);
            await _userManager.UpdateAsync(user);
        }

        _notificationService.CreateNotification(new CreateNotificationDTO
        {
            UserId = user.Id,
            Title = "New sign-in to your account",
            Message = $"You signed in on {DateTime.UtcNow:dd MMM yyyy} at {DateTime.UtcNow:HH:mm} UTC. If this wasn't you, secure your account immediately.",
            Type = NotificationType.System
        });

        var tokenResult = await _jwtServices.GenerateTokenAsync(user);
        return BuildAuthResponse(user, tokenResult, activeRefreshToken);
    }

    public async Task<ActionResponseDTO> AddRoleAsync(AddRoleDTO model)
    {
        if (!ValidRoles.Contains(model.Role))
        {
            return new ActionResponseDTO { Succeeded = false, Message = "Role must be Affiliate, Merchant, or Admin" };
        }

        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null)
        {
            return new ActionResponseDTO { Succeeded = false, Message = "Invalid user id" };
        }

        if (!await _roleManager.RoleExistsAsync(model.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(model.Role));
        }

        if (await _userManager.IsInRoleAsync(user, model.Role))
        {
            return new ActionResponseDTO { Succeeded = false, Message = "User is already assigned to this role" };
        }

        var result = await _userManager.AddToRoleAsync(user, model.Role);
        if (!result.Succeeded)
        {
            return new ActionResponseDTO
            {
                Succeeded = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        return new ActionResponseDTO { Succeeded = true, Message = "Role assigned successfully" };
    }

    public async Task<ActionResponseDTO> ChangePasswordAsync(string userId, ChangePasswordDTO model)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return new ActionResponseDTO
            {
                Succeeded = false,
                Message = "User not found"
            };
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            return new ActionResponseDTO
            {
                Succeeded = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        return new ActionResponseDTO
        {
            Succeeded = true,
            Message = "Password changed successfully"
        };
    }

    public async Task<List<RoleReadDTO>> GetRolesAsync()
    {
        return await _roleManager.Roles
            .Select(r => new RoleReadDTO
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty,
                NormalizedName = r.NormalizedName
            })
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<AuthResponseDTO> RefreshTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthResponseDTO { Message = "Invalid token" };
        }

        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
        if (user is null)
        {
            return new AuthResponseDTO { Message = "Invalid token" };
        }

        var refreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == token);
        if (refreshToken is null || !refreshToken.IsActive)
        {
            return new AuthResponseDTO { Message = "Inactive token" };
        }

        refreshToken.RevokedOn = DateTime.UtcNow;
        var newRefreshToken = GenerateRefreshToken();
        user.RefreshTokens.Add(newRefreshToken);
        await _userManager.UpdateAsync(user);

        var tokenResult = await _jwtServices.GenerateTokenAsync(user);
        return BuildAuthResponse(user, tokenResult, newRefreshToken);
    }

    public async Task<ActionResponseDTO> RevokeTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new ActionResponseDTO { Succeeded = false, Message = "Token is required" };
        }

        var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
        if (user is null)
        {
            return new ActionResponseDTO { Succeeded = false, Message = "Token is invalid" };
        }

        var refreshToken = user.RefreshTokens.SingleOrDefault(t => t.Token == token);
        if (refreshToken is null || !refreshToken.IsActive)
        {
            return new ActionResponseDTO { Succeeded = false, Message = "Token is invalid" };
        }

        refreshToken.RevokedOn = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return new ActionResponseDTO { Succeeded = true, Message = "Token revoked successfully" };
    }

    private static RefreshToken GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            ExpiresOn = DateTime.UtcNow.AddDays(10),
            CreatedOn = DateTime.UtcNow
        };
    }

    private static AuthResponseDTO BuildAuthResponse(AppUser user, TokenResultDTO tokenResult, RefreshToken refreshToken)
    {
        return new AuthResponseDTO
        {
            IsAuthenticated = true,
            Message = "Success",
            UserId = user.Id,
            FullName = user.FullName,
            UserName = user.UserName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = tokenResult.Roles,
            Token = tokenResult.Token,
            Expiration = tokenResult.ExpiresOn,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiration = refreshToken.ExpiresOn
        };
    }
}
