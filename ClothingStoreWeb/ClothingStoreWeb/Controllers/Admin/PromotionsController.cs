using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.Repository;
using ClothingStoreWeb.Services;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>CRUD Khuyến mãi (/Admin/Promotions)</summary>
[Route("Admin/[controller]")]
public class PromotionsController : AdminControllerBase
{
    private readonly IRepository<Promotion> _repo;
    private readonly ILookupService _lookup;
    private readonly IFileService _files;

    private static readonly string[] CreateFields = { "Code", "Description", "DiscountType", "DiscountValue", "MinOrderValue", "MaxDiscountAmount", "StartDate", "EndDate", "UsageLimit", "UsedCount" };
    private static readonly string[] EditFields = { "Code", "Description", "DiscountType", "DiscountValue", "MinOrderValue", "MaxDiscountAmount", "StartDate", "EndDate", "UsageLimit", "UsedCount" };

    public PromotionsController(IRepository<Promotion> repo, ILookupService lookup, IFileService files)
    {
        _repo = repo;
        _lookup = lookup;
        _files = files;
    }

    private async Task LoadLookups()
    {
        ViewBag.DiscountTypeOptions = new List<SelectListItem> { new SelectListItem("Phần trăm (%)", "0"), new SelectListItem("Số tiền cố định (₫)", "1") };
        await Task.CompletedTask;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = _repo.Table.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(x => x.PromotionId.Contains(q) || x.Code.Contains(q) || (x.Description != null && x.Description.Contains(q)));
        }
        var model = await PagedList<Promotion>.CreateAsync(query.OrderBy(x => x.PromotionId), page);
        ViewBag.Q = q;
        await LoadLookups();
        return View(model);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View("Form", new Promotion());
    }

    [HttpPost("Create"), ActionName("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost()
    {
        var m = new Promotion();
        await BindAsync(m, CreateFields);
        {
            var ids = await _repo.Table.Select(x => x.PromotionId).ToListAsync();
            m.PromotionId = NextId(ids, "PROM", 2);
        }
        if (!string.IsNullOrWhiteSpace(m.Code) && await _repo.Table.AnyAsync(x => x.Code == m.Code))
            ModelState.AddModelError("Code", "Giá trị này đã tồn tại.");

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

        Success("Đã thêm khuyến mãi thành công.");
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
        if (!string.IsNullOrWhiteSpace(m.Code) && await _repo.Table.AnyAsync(x => x.Code == m.Code && x.PromotionId != id))
            ModelState.AddModelError("Code", "Giá trị này đã tồn tại.");

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

        Success("Đã cập nhật khuyến mãi.");
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
            Success("Đã xóa khuyến mãi.");
        }
        catch (DbUpdateException)
        {
            Fail("Không thể xóa vì dữ liệu này đang được sử dụng ở nơi khác (khóa ngoại).");
        }
        return RedirectToAction(nameof(Index));
    }
}
