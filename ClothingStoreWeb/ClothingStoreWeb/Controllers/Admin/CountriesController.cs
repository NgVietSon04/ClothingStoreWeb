using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.Repository;
using ClothingStoreWeb.Services;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>CRUD Quốc gia (/Admin/Countries)</summary>
[Route("Admin/[controller]")]
public class CountriesController : AdminControllerBase
{
    private readonly IRepository<Country> _repo;
    private readonly ILookupService _lookup;
    private readonly IFileService _files;

    private static readonly string[] CreateFields = { "CountryId", "CountryName" };
    private static readonly string[] EditFields = { "CountryName" };

    public CountriesController(IRepository<Country> repo, ILookupService lookup, IFileService files)
    {
        _repo = repo;
        _lookup = lookup;
        _files = files;
    }

    private async Task LoadLookups()
    {

        await Task.CompletedTask;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = _repo.Table.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x => x.CountryId.Contains(q) || (x.CountryName != null && x.CountryName.Contains(q)));
        }
        var model = await PagedList<Country>.CreateAsync(query.OrderBy(x => x.CountryId), page);
        ViewBag.Q = q;
        await LoadLookups();
        return View(model);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View("Form", new Country());
    }

    [HttpPost("Create"), ActionName("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost()
    {
        var m = new Country();
        await BindAsync(m, CreateFields);
        if (!string.IsNullOrWhiteSpace(m.CountryId) && await _repo.Table.AnyAsync(x => x.CountryId == m.CountryId))
            ModelState.AddModelError("CountryId", "Mã này đã tồn tại.");

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

        Success("Đã thêm quốc gia thành công.");
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

        Success("Đã cập nhật quốc gia.");
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
            Success("Đã xóa quốc gia.");
        }
        catch (DbUpdateException)
        {
            Fail("Không thể xóa vì dữ liệu này đang được sử dụng ở nơi khác (khóa ngoại).");
        }
        return RedirectToAction(nameof(Index));
    }
}
