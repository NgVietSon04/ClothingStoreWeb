using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.Services;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>CRUD sản phẩm + thư viện ảnh (/Admin/Products). Biến thể quản lý ở ProductVariantsController.</summary>
[Route("Admin/[controller]")]
public class ProductsController : AdminControllerBase
{
    private readonly ClothingStoreDbContext _db;
    private readonly ILookupService _lookup;
    private readonly IFileService _files;

    private static readonly string[] Fields =
    {
        "ProductName", "MaterialId", "FitType", "StyleCode", "Weight", "Season", "BrandId", "OriginCountryId",
        "CategoryId", "AudienceId", "WarrantyDays", "DiscountPercent", "Website", "CareInstructions",
        "Description", "Thumbnail"
    };

    public ProductsController(ClothingStoreDbContext db, ILookupService lookup, IFileService files)
    {
        _db = db;
        _lookup = lookup;
        _files = files;
    }

    private async Task LoadLookups()
    {
        ViewBag.MaterialIdOptions = await _lookup.MaterialsAsync();
        ViewBag.BrandIdOptions = await _lookup.BrandsAsync();
        ViewBag.OriginCountryIdOptions = await _lookup.CountriesAsync();
        ViewBag.CategoryIdOptions = await _lookup.CategoriesAsync();
        ViewBag.AudienceIdOptions = await _lookup.AudiencesAsync();
        ViewBag.FitTypeOptions = new List<SelectListItem>
        {
            new SelectListItem("Regular Fit", "Regular Fit"), new SelectListItem("Slim Fit", "Slim Fit"),
            new SelectListItem("Oversize", "Oversize"), new SelectListItem("Loose Fit", "Loose Fit")
        };
        ViewBag.SeasonOptions = new List<SelectListItem>
        {
            new SelectListItem("Xuân Hè", "Xuân Hè"), new SelectListItem("Thu Đông", "Thu Đông"),
            new SelectListItem("Bốn mùa", "Bốn mùa"), new SelectListItem("Đông", "Đông")
        };
    }

    // ------------------------------------------------------------ DANH SÁCH
    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, string? categoryId, string? brandId, string? audienceId, int page = 1)
    {
        var query = _db.Products.AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => p.ProductId.Contains(q)
                                     || (p.ProductName != null && p.ProductName.Contains(q))
                                     || (p.StyleCode != null && p.StyleCode.Contains(q)));
        }
        if (!string.IsNullOrEmpty(categoryId)) query = query.Where(p => p.CategoryId == categoryId);
        if (!string.IsNullOrEmpty(brandId)) query = query.Where(p => p.BrandId == brandId);
        if (!string.IsNullOrEmpty(audienceId)) query = query.Where(p => p.AudienceId == audienceId);

        var model = await PagedList<Product>.CreateAsync(query.OrderBy(p => p.ProductId), page);

        ViewBag.Q = q;
        ViewBag.CategoryId = categoryId;
        ViewBag.BrandId = brandId;
        ViewBag.AudienceId = audienceId;
        ViewBag.Categories = await _lookup.CategoriesAsync();
        ViewBag.Brands = await _lookup.BrandsAsync();
        ViewBag.Audiences = await _lookup.AudiencesAsync();
        return View(model);
    }

    // ------------------------------------------------------------ CHI TIẾT
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(string id)
    {
        var p = await _db.Products.AsNoTracking()
            .Include(x => x.Brand)
            .Include(x => x.Category)
            .Include(x => x.Material)
            .Include(x => x.Audience)
            .Include(x => x.OriginCountry)
            .Include(x => x.Images.OrderBy(i => i.SortOrder))
            .Include(x => x.Variants).ThenInclude(v => v.Size)
            .Include(x => x.Variants).ThenInclude(v => v.Color)
            .FirstOrDefaultAsync(x => x.ProductId == id);

        if (p == null) return NotFound();
        return View(p);
    }

    // ------------------------------------------------------------ THÊM
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await LoadLookups();
        return View("Form", new Product());
    }

    [HttpPost("Create"), ActionName("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost()
    {
        var m = new Product();
        await BindAsync(m, Fields);
        m.ProductId = NextId(await _db.Products.Select(x => x.ProductId).ToListAsync(), "SP", 3);

        var up = await UploadAsync(_files, "Thumbnail_file");
        if (up != null) m.Thumbnail = up;

        if (!ModelState.IsValid)
        {
            await LoadLookups();
            return View("Form", m);
        }

        _db.Products.Add(m);
        await _db.SaveChangesAsync();
        Success("Đã thêm sản phẩm. Hãy thêm các biến thể (size / màu / tồn kho) cho sản phẩm này.");
        return RedirectToAction(nameof(Details), new { id = m.ProductId });
    }

    // ------------------------------------------------------------ SỬA
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        var m = await _db.Products.FindAsync(id);
        if (m == null) return NotFound();
        await LoadLookups();
        return View("Form", m);
    }

    [HttpPost("Edit/{id}"), ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(string id)
    {
        var m = await _db.Products.FindAsync(id);
        if (m == null) return NotFound();

        await BindAsync(m, Fields);
        var up = await UploadAsync(_files, "Thumbnail_file");
        if (up != null) m.Thumbnail = up;

        if (!ModelState.IsValid)
        {
            await LoadLookups();
            return View("Form", m);
        }

        await _db.SaveChangesAsync();
        Success("Đã cập nhật sản phẩm.");
        return RedirectToAction(nameof(Details), new { id });
    }

    // ------------------------------------------------------------ XÓA
    [HttpPost("Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var p = await _db.Products.Include(x => x.Images).FirstOrDefaultAsync(x => x.ProductId == id);
        if (p == null) return NotFound();

        if (await _db.ProductVariants.AnyAsync(v => v.ProductId == id))
        {
            Fail("Sản phẩm còn biến thể. Hãy xóa các biến thể trước khi xóa sản phẩm.");
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _db.ProductImages.RemoveRange(p.Images);
            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
            Success("Đã xóa sản phẩm.");
        }
        catch (DbUpdateException)
        {
            Fail("Không thể xóa sản phẩm vì đang có đánh giá / yêu thích tham chiếu tới.");
        }
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------ THƯ VIỆN ẢNH
    [HttpPost("AddImages/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddImages(string id, List<IFormFile> files)
    {
        if (!await _db.Products.AnyAsync(p => p.ProductId == id)) return NotFound();

        if (files == null || files.Count == 0 || files.All(f => f.Length == 0))
        {
            Fail("Vui lòng chọn ít nhất một ảnh.");
            return RedirectToAction(nameof(Details), new { id });
        }

        int sort = await _db.ProductImages.Where(i => i.ProductId == id).MaxAsync(i => i.SortOrder) ?? 0;
        var added = 0;
        foreach (var f in files)
        {
            try
            {
                var name = await _files.SaveImageAsync(f);
                if (name == null) continue;
                sort++;
                _db.ProductImages.Add(new ProductImage { ProductId = id, ImageFileName = name, SortOrder = (short)sort });
                added++;
            }
            catch (InvalidOperationException ex)
            {
                Fail($"{f.FileName}: {ex.Message}");
            }
        }
        await _db.SaveChangesAsync();
        if (added > 0) Success($"Đã thêm {added} ảnh.");
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("DeleteImage"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(string productId, string fileName)
    {
        var img = await _db.ProductImages.FindAsync(productId, fileName);
        if (img == null) return NotFound();

        _db.ProductImages.Remove(img);
        await _db.SaveChangesAsync();

        // Chỉ xóa file vật lý khi không còn nơi nào dùng tên file này
        var used = await _db.ProductImages.AnyAsync(i => i.ImageFileName == fileName)
                   || await _db.Products.AnyAsync(p => p.Thumbnail == fileName)
                   || await _db.ProductVariants.AnyAsync(v => v.Thumbnail == fileName);
        if (!used) _files.Delete(fileName);

        Success("Đã xóa ảnh.");
        return RedirectToAction(nameof(Details), new { id = productId });
    }

    [HttpPost("SetThumbnail"), ValidateAntiForgeryToken]
    public async Task<IActionResult> SetThumbnail(string productId, string fileName)
    {
        var p = await _db.Products.FindAsync(productId);
        if (p == null) return NotFound();
        p.Thumbnail = fileName;
        await _db.SaveChangesAsync();
        Success("Đã đặt làm ảnh đại diện.");
        return RedirectToAction(nameof(Details), new { id = productId });
    }
}
