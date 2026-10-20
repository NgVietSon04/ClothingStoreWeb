using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.Services;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>CRUD biến thể sản phẩm (size / màu / giá / tồn kho) và ảnh biến thể (/Admin/ProductVariants).</summary>
[Route("Admin/[controller]")]
public class ProductVariantsController : AdminControllerBase
{
    private const int LowStock = 10;

    private readonly ClothingStoreDbContext _db;
    private readonly ILookupService _lookup;
    private readonly IFileService _files;
    private readonly IProductService _products;

    private static readonly string[] Fields =
        { "ProductId", "SizeId", "ColorId", "UnitPrice", "Discount", "StockQuantity", "Thumbnail", "VideoUrl" };

    public ProductVariantsController(ClothingStoreDbContext db, ILookupService lookup, IFileService files, IProductService products)
    {
        _db = db;
        _lookup = lookup;
        _files = files;
        _products = products;
    }

    private async Task LoadLookups()
    {
        ViewBag.ProductIdOptions = await _lookup.ProductsAsync();
        ViewBag.SizeIdOptions = await _lookup.SizesAsync();
        ViewBag.ColorIdOptions = await _lookup.ColorsAsync();
    }

    // ------------------------------------------------------------ DANH SÁCH / KHO
    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, string? productId, bool lowStock = false, int page = 1)
    {
        var query = _db.ProductVariants.AsNoTracking()
            .Include(v => v.Product)
            .Include(v => v.Size)
            .Include(v => v.Color)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(v => v.VariantId.Contains(q)
                                     || (v.Product != null && v.Product.ProductName != null && v.Product.ProductName.Contains(q)));
        }
        if (!string.IsNullOrEmpty(productId)) query = query.Where(v => v.ProductId == productId);
        if (lowStock) query = query.Where(v => v.StockQuantity <= LowStock);

        var model = await PagedList<ProductVariant>.CreateAsync(query.OrderBy(v => v.VariantId), page);
        ViewBag.Q = q;
        ViewBag.ProductId = productId;
        ViewBag.LowStock = lowStock;
        ViewBag.Products = await _lookup.ProductsAsync();
        return View(model);
    }

    // ------------------------------------------------------------ THÊM
    [HttpGet("Create")]
    public async Task<IActionResult> Create(string? productId)
    {
        await LoadLookups();
        return View("Form", new ProductVariant { ProductId = productId });
    }

    [HttpPost("Create"), ActionName("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost()
    {
        var m = new ProductVariant();
        await BindAsync(m, Fields);
        await ValidateAsync(m, null);

        var up = await UploadAsync(_files, "Thumbnail_file");
        if (up != null) m.Thumbnail = up;

        if (!ModelState.IsValid)
        {
            await LoadLookups();
            return View("Form", m);
        }

        m.VariantId = await MakeVariantIdAsync(m.ProductId!, m.SizeId!, m.ColorId!);
        _db.ProductVariants.Add(m);
        await _db.SaveChangesAsync();
        await _products.RecalcPriceRangeAsync(m.ProductId);

        Success("Đã thêm biến thể " + m.VariantId + ".");
        return RedirectToAction("Details", "Products", new { id = m.ProductId });
    }

    // ------------------------------------------------------------ SỬA
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        var m = await _db.ProductVariants.Include(v => v.Images.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(v => v.VariantId == id);
        if (m == null) return NotFound();
        await LoadLookups();
        return View("Form", m);
    }

    [HttpPost("Edit/{id}"), ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(string id)
    {
        var m = await _db.ProductVariants.Include(v => v.Images.OrderBy(i => i.SortOrder))
            .FirstOrDefaultAsync(v => v.VariantId == id);
        if (m == null) return NotFound();

        var oldProductId = m.ProductId;
        await BindAsync(m, Fields);
        await ValidateAsync(m, id);

        var up = await UploadAsync(_files, "Thumbnail_file");
        if (up != null) m.Thumbnail = up;

        if (!ModelState.IsValid)
        {
            await LoadLookups();
            return View("Form", m);
        }

        await _db.SaveChangesAsync();
        await _products.RecalcPriceRangeAsync(m.ProductId);
        if (oldProductId != m.ProductId) await _products.RecalcPriceRangeAsync(oldProductId);

        Success("Đã cập nhật biến thể.");
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------ XÓA
    [HttpPost("Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var v = await _db.ProductVariants.Include(x => x.Images).FirstOrDefaultAsync(x => x.VariantId == id);
        if (v == null) return NotFound();

        var productId = v.ProductId;
        try
        {
            _db.ProductVariantImages.RemoveRange(v.Images);
            _db.ProductVariants.Remove(v);
            await _db.SaveChangesAsync();
            await _products.RecalcPriceRangeAsync(productId);
            Success("Đã xóa biến thể.");
        }
        catch (DbUpdateException)
        {
            Fail("Không thể xóa: biến thể này đã xuất hiện trong đơn hàng hoặc phiếu nhập kho.");
        }
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------ ẢNH BIẾN THỂ
    [HttpPost("AddImages/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> AddImages(string id, List<IFormFile> files)
    {
        if (!await _db.ProductVariants.AnyAsync(v => v.VariantId == id)) return NotFound();

        if (files == null || files.Count == 0 || files.All(f => f.Length == 0))
        {
            Fail("Vui lòng chọn ít nhất một ảnh.");
            return RedirectToAction(nameof(Edit), new { id });
        }

        int sort = await _db.ProductVariantImages.Where(i => i.VariantId == id).MaxAsync(i => i.SortOrder) ?? 0;
        var added = 0;
        foreach (var f in files)
        {
            try
            {
                var name = await _files.SaveImageAsync(f);
                if (name == null) continue;
                sort++;
                _db.ProductVariantImages.Add(new ProductVariantImage { VariantId = id, ImageFileName = name, SortOrder = (short)sort });
                added++;
            }
            catch (InvalidOperationException ex)
            {
                Fail($"{f.FileName}: {ex.Message}");
            }
        }
        await _db.SaveChangesAsync();
        if (added > 0) Success($"Đã thêm {added} ảnh.");
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("DeleteImage"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteImage(string variantId, string fileName)
    {
        var img = await _db.ProductVariantImages.FindAsync(variantId, fileName);
        if (img == null) return NotFound();

        _db.ProductVariantImages.Remove(img);
        await _db.SaveChangesAsync();

        var used = await _db.ProductVariantImages.AnyAsync(i => i.ImageFileName == fileName)
                   || await _db.ProductImages.AnyAsync(i => i.ImageFileName == fileName)
                   || await _db.ProductVariants.AnyAsync(v => v.Thumbnail == fileName)
                   || await _db.Products.AnyAsync(p => p.Thumbnail == fileName);
        if (!used) _files.Delete(fileName);

        Success("Đã xóa ảnh.");
        return RedirectToAction(nameof(Edit), new { id = variantId });
    }

    // ------------------------------------------------------------ HELPER
    private async Task ValidateAsync(ProductVariant m, string? selfId)
    {
        if (m.UnitPrice is < 0) ModelState.AddModelError("UnitPrice", "Đơn giá không được âm.");
        if (m.Discount is < 0 or > 100) ModelState.AddModelError("Discount", "Giảm giá phải từ 0 đến 100 (%).");
        if (m.StockQuantity is < 0) ModelState.AddModelError("StockQuantity", "Tồn kho không được âm.");

        if (!string.IsNullOrEmpty(m.ProductId) && !string.IsNullOrEmpty(m.SizeId) && !string.IsNullOrEmpty(m.ColorId))
        {
            var dup = await _db.ProductVariants.AnyAsync(v =>
                v.ProductId == m.ProductId && v.SizeId == m.SizeId && v.ColorId == m.ColorId && v.VariantId != selfId);
            if (dup) ModelState.AddModelError("", "Sản phẩm này đã có biến thể với cùng size và màu.");
        }
    }

    private async Task<string> MakeVariantIdAsync(string productId, string sizeId, string colorId)
    {
        var color = colorId.Length >= 3 ? colorId.Substring(0, 3) : colorId;
        var baseId = $"{productId}-{sizeId}-{color}".ToUpperInvariant();
        if (baseId.Length > 22) baseId = baseId.Substring(0, 22);

        var id = baseId;
        var i = 2;
        while (await _db.ProductVariants.AnyAsync(v => v.VariantId == id))
        {
            id = baseId + "-" + i;
            i++;
        }
        return id;
    }
}
