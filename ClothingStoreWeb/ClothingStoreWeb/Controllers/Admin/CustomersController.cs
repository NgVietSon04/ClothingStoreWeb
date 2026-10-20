using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.Repository;
using ClothingStoreWeb.Services;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>CRUD Khách hàng (/Admin/Customers)</summary>
[Route("Admin/[controller]")]
public class CustomersController : AdminControllerBase
{
    private readonly IRepository<Customer> _repo;
    private readonly ILookupService _lookup;
    private readonly IFileService _files;

    private static readonly string[] CreateFields = { "Avatar", "FullName", "Username", "DateOfBirth", "PhoneNumber", "Address", "CustomerType", "Notes" };
    private static readonly string[] EditFields = { "Avatar", "FullName", "Username", "DateOfBirth", "PhoneNumber", "Address", "CustomerType", "Notes" };

    public CustomersController(IRepository<Customer> repo, ILookupService lookup, IFileService files)
    {
        _repo = repo;
        _lookup = lookup;
        _files = files;
    }

    private async Task LoadLookups()
    {
        ViewBag.UsernameOptions = await _lookup.UsersAsync();
        ViewBag.CustomerTypeOptions = new List<SelectListItem> { new SelectListItem("Thường", "0"), new SelectListItem("Thân thiết", "1"), new SelectListItem("VIP", "2") };
        await Task.CompletedTask;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = _repo.Table.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x => x.CustomerId.Contains(q) || (x.FullName != null && x.FullName.Contains(q)) || (x.PhoneNumber != null && x.PhoneNumber.Contains(q)) || (x.Username != null && x.Username.Contains(q)));
        }
        var model = await PagedList<Customer>.CreateAsync(query.OrderBy(x => x.CustomerId), page);
        ViewBag.Q = q;
        await LoadLookups();
        return View(model);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View("Form", new Customer());
    }

    [HttpPost("Create"), ActionName("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost()
    {
        var m = new Customer();
        await BindAsync(m, CreateFields);
        {
            var ids = await _repo.Table.Select(x => x.CustomerId).ToListAsync();
            m.CustomerId = NextId(ids, "KH", 3);
        }
        var up_Avatar = await UploadAsync(_files, "Avatar_file");
        if (up_Avatar != null) m.Avatar = up_Avatar;
        if (!ModelState.IsValid)
        {
            await LoadLookups();
            return View("Form", m);
        }

        try
        {
            await _repo.AddAsync(m);
            await _repo.SaveAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể lưu dữ liệu. Vui lòng kiểm tra lại các thông tin liên kết.");
            await LoadLookups();
            return View("Form", m);
        }

        Success("Đã thêm khách hàng thành công.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        var m = await _repo.FindAsync(id);
        if (m == null) return NotFound();
        await LoadLookups();
        return View("Form", m);
    }

    [HttpPost("Edit/{id}"), ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(string id)
    {
        var m = await _repo.FindAsync(id);
        if (m == null) return NotFound();

        await BindAsync(m, EditFields);

        var up_Avatar = await UploadAsync(_files, "Avatar_file");
        if (up_Avatar != null) m.Avatar = up_Avatar;
        if (!ModelState.IsValid)
        {
            await LoadLookups();
            return View("Form", m);
        }

        try
        {
            await _repo.SaveAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể lưu dữ liệu. Vui lòng kiểm tra lại các thông tin liên kết.");
            await LoadLookups();
            return View("Form", m);
        }

        Success("Đã cập nhật khách hàng.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var m = await _repo.FindAsync(id);
        if (m == null) return NotFound();
        try
        {
            _repo.Remove(m);
            await _repo.SaveAsync();
            Success("Đã xóa khách hàng.");
        }
        catch (DbUpdateException)
        {
            Fail("Không thể xóa vì dữ liệu này đang được sử dụng ở nơi khác (khóa ngoại).");
        }
        return RedirectToAction(nameof(Index));
    }
}
