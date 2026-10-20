using ClothingStoreWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.Repository;
using ClothingStoreWeb.Services;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>CRUD Bài viết (Blog) (/Admin/Blogs)</summary>
[Route("Admin/[controller]")]
public class BlogsController : AdminControllerBase
{
    private readonly IRepository<Blog> _repo;
    private readonly ILookupService _lookup;
    private readonly IFileService _files;

    private static readonly string[] CreateFields = { "Title", "Description", "Thumbnail", "CommentCount" };
    private static readonly string[] EditFields = { "Title", "Description", "Thumbnail", "CommentCount" };

    public BlogsController(IRepository<Blog> repo, ILookupService lookup, IFileService files)
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
            query = query.Where(x => (x.Title != null && x.Title.Contains(q)) || (x.Description != null && x.Description.Contains(q)));
        }
        var model = await PagedList<Blog>.CreateAsync(query.OrderByDescending(x => x.BlogId), page);
        ViewBag.Q = q;
        await LoadLookups();
        return View(model);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View("Form", new Blog());
    }

    [HttpPost("Create"), ActionName("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost()
    {
        var m = new Blog();
        await BindAsync(m, CreateFields);

        var up_Thumbnail = await UploadAsync(_files, "Thumbnail_file");
        if (up_Thumbnail != null) m.Thumbnail = up_Thumbnail;
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

        Success("Đã thêm bài viết (blog) thành công.");
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var m = await _repo.FindAsync(id);
        if (m == null) return NotFound();
        await LoadLookups();
        return View("Form", m);
    }

    [HttpPost("Edit/{id}"), ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id)
    {
        var m = await _repo.FindAsync(id);
        if (m == null) return NotFound();

        await BindAsync(m, EditFields);

        var up_Thumbnail = await UploadAsync(_files, "Thumbnail_file");
        if (up_Thumbnail != null) m.Thumbnail = up_Thumbnail;
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

        Success("Đã cập nhật bài viết (blog).");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var m = await _repo.FindAsync(id);
        if (m == null) return NotFound();
        try
        {
            _repo.Remove(m);
            await _repo.SaveAsync();
            Success("Đã xóa bài viết (blog).");
        }
        catch (DbUpdateException)
        {
            Fail("Không thể xóa vì dữ liệu này đang được sử dụng ở nơi khác (khóa ngoại).");
        }
        return RedirectToAction(nameof(Index));
    }
}
