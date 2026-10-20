using System.Text.RegularExpressions;

namespace ClothingStoreWeb.Services;

public interface IFileService
{
    /// <summary>Lưu ảnh vào wwwroot/uploads, trả về tên file. Ném InvalidOperationException nếu file không hợp lệ.</summary>
    Task<string?> SaveImageAsync(IFormFile? file);

    /// <summary>Xóa file trong wwwroot/uploads (nếu có). An toàn với tên file không tồn tại.</summary>
    void Delete(string? fileName);
}

public class FileService : IFileService
{
    private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long MaxBytes = 5 * 1024 * 1024;
    private readonly string _folder;

    public FileService(IWebHostEnvironment env)
    {
        _folder = Path.Combine(env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot"), "uploads");
    }

    public async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(ext))
            throw new InvalidOperationException("Chỉ chấp nhận ảnh .jpg, .jpeg, .png, .webp, .gif");
        if (file.Length > MaxBytes)
            throw new InvalidOperationException("Ảnh tối đa 5 MB");

        Directory.CreateDirectory(_folder);

        var baseName = Regex.Replace(Path.GetFileNameWithoutExtension(file.FileName).ToLowerInvariant(), @"[^a-z0-9\-]+", "-").Trim('-');
        if (baseName.Length == 0) baseName = "img";
        if (baseName.Length > 50) baseName = baseName.Substring(0, 50);
        var name = $"{baseName}-{Guid.NewGuid().ToString("N").Substring(0, 8)}{ext}";

        await using var fs = new FileStream(Path.Combine(_folder, name), FileMode.Create);
        await file.CopyToAsync(fs);
        return name;
    }

    public void Delete(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return;
        var path = Path.Combine(_folder, Path.GetFileName(fileName));
        if (File.Exists(path))
        {
            try { File.Delete(path); } catch (IOException) { /* bỏ qua */ }
        }
    }
}
