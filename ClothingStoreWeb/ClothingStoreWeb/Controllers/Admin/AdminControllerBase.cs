using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClothingStoreWeb.Services;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>Lớp cơ sở cho mọi controller của khu vực Admin: bắt buộc đăng nhập bằng cookie admin.</summary>
[Authorize(AuthenticationSchemes = AdminAuth.Scheme, Roles = AdminAuth.Role)]
public abstract class AdminControllerBase : Controller
{
    protected void Success(string message) => TempData["Ok"] = message;

    protected void Fail(string message) => TempData["Err"] = message;

    /// <summary>Mã nhân viên của người đang đăng nhập (nếu tài khoản có hồ sơ nhân viên).</summary>
    protected string? CurrentEmployeeId => User.FindFirst("EmployeeId")?.Value;

    /// <summary>Sinh mã kế tiếp, ví dụ NextId(["SP001","SP024"], "SP", 3) => "SP025".</summary>
    protected static string NextId(IEnumerable<string?> ids, string prefix, int pad)
    {
        var max = 0;
        foreach (var id in ids)
        {
            if (id == null || !id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
            if (int.TryParse(id.Substring(prefix.Length), out var n) && n > max) max = n;
        }
        return prefix + (max + 1).ToString("D" + pad);
    }

    /// <summary>
    /// Bind form vào model nhưng CHỈ các thuộc tính trong danh sách (chống over-posting),
    /// đồng thời bỏ qua lỗi validate của các thuộc tính không nằm trong danh sách.
    /// </summary>
    protected async Task<bool> BindAsync<T>(T model, string[] fields) where T : class
    {
        await TryUpdateModelAsync(model, "", mm => mm.PropertyName != null && fields.Contains(mm.PropertyName));
        foreach (var key in ModelState.Keys.ToList())
        {
            if (!fields.Contains(key)) ModelState.Remove(key);
        }
        return ModelState.IsValid;
    }

    /// <summary>Lưu file ảnh từ input type=file (tên formName). Lỗi sẽ được ghi vào ModelState.</summary>
    protected async Task<string?> UploadAsync(IFileService files, string formName)
    {
        var file = Request.Form.Files.GetFile(formName);
        if (file == null || file.Length == 0) return null;
        try
        {
            return await files.SaveImageAsync(file);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(formName.Replace("_file", ""), ex.Message);
            return null;
        }
    }
}
