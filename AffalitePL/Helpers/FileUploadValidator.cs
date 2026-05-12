using System.ComponentModel.DataAnnotations;
using System.Security;

namespace AffalitePL.Helpers;

public static class FileUploadValidator
{
    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    };

    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public static ValidationResult ValidateImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return new ValidationResult("File is empty.");

        if (file.Length > MaxFileSize)
            return new ValidationResult($"File size exceeds the limit of {MaxFileSize / 1024 / 1024} MB.");

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrEmpty(extension) || !AllowedImageExtensions.Contains(extension))
            return new ValidationResult($"Invalid file type. Allowed types: {string.Join(", ", AllowedImageExtensions)}.");

        return ValidationResult.Success!;
    }

    public static async Task<string> SaveImageAsync(IFormFile file, string folderPath)
    {
        var validation = ValidateImage(file);
        if (validation != ValidationResult.Success)
            throw new ArgumentException(validation.ErrorMessage);

        Directory.CreateDirectory(folderPath);

        var safeFileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
        var fullPath = Path.Combine(folderPath, safeFileName);

        // Prevent path traversal
        var resolvedPath = Path.GetFullPath(fullPath);
        var resolvedFolder = Path.GetFullPath(folderPath);
        if (!resolvedPath.StartsWith(resolvedFolder, StringComparison.OrdinalIgnoreCase))
            throw new SecurityException("Invalid file path.");

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return safeFileName;
    }
}
