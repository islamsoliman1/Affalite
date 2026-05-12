using AffaliteDAL.Data;
using AffaliteDAL.Entities;
using AffaliteDAL.IRepo;
using Microsoft.EntityFrameworkCore;

namespace AffaliteDAL.Repo;

public class OrderRepo : IOrderRepo
{
    private readonly AffaliteDBContext _context;

    public OrderRepo(AffaliteDBContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Commission)
                .ThenInclude(c => c!.MerchantCommissions)
            .Include(o => o.MerchantOrder)
                .ThenInclude(m => m.Merchant)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images)
            .Include(o => o.Affiliate)
                .ThenInclude(a => a!.AppUser)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByAffIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Commission)
                .ThenInclude(c => c!.MerchantCommissions)
            .Include(o => o.MerchantOrder)
                .ThenInclude(m => m.Merchant)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images)
            .Where(o => o.AffiliateId == id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByMerIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Include(o => o.Commission)
                .ThenInclude(c => c!.MerchantCommissions)
            .Include(o => o.MerchantOrder)
                .ThenInclude(m => m.Merchant)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p!.Images)
            .Where(o => o.MerchantOrder.Any(m => m.MerchantId == id))
            .ToListAsync(cancellationToken);
    }
}
