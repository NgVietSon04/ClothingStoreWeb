using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.Repository;
using ClothingStoreWeb.Services;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>CRUD Nhân viên (/Admin/Employees)</summary>
[Route("Admin/[controller]")]
public class EmployeesController : AdminControllerBase
{
    private readonly IRepository<Employee> _repo;
    private readonly ILookupService _lookup;
    private readonly IFileService _files;

    private static readonly string[] CreateFields = { "Avatar", "FullName", "Username", "DateOfBirth", "PhoneNumber1", "PhoneNumber2", "Address", "Position", "Notes" };
    private static readonly string[] EditFields = { "Avatar", "FullName", "Username", "DateOfBirth", "PhoneNumber1", "PhoneNumber2", "Address", "Position", "Notes" };

    public EmployeesController(IRepository<Employee> repo, ILookupService lookup, IFileService files)
    {
        _repo = repo;
        _lookup = lookup;
        _files = files;
    }

    private async Task LoadLookups()
    {
        ViewBag.UsernameOptions = await _lookup.UsersAsync();
        await Task.CompletedTask;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = _repo.Table.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x => x.EmployeeId.Contains(q) || (x.FullName != null && x.FullName.Contains(q)) || (x.PhoneNumber1 != null && x.PhoneNumber1.Contains(q)) || (x.Position != null && x.Position.Contains(q)));
        }
        var model = await PagedList<Employee>.CreateAsync(query.OrderBy(x => x.EmployeeId), page);
        ViewBag.Q = q;
        await LoadLookups();
        return View(model);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View("Form", new Employee());
    }

    [HttpPost("Create"), ActionName("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost()
    {
        var m = new Employee();
        await BindAsync(m, CreateFields);
        {
            var ids = await _repo.Table.Select(x => x.EmployeeId).ToListAsync();
            m.EmployeeId = NextId(ids, "NV", 3);
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

        Success("Đã thêm nhân viên thành công.");
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

        Success("Đã cập nhật nhân viên.");
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
            Success("Đã xóa nhân viên.");
        }
        catch (DbUpdateException)
        {
            Fail("Không thể xóa vì dữ liệu này đang được sử dụng ở nơi khác (khóa ngoại).");
        }
        return RedirectToAction(nameof(Index));
    }
}
