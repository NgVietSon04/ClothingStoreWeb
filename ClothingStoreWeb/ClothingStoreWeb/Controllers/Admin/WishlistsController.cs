using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>Xem / xóa danh sách yêu thích của khách (/Admin/Wishlists).</summary>
[Route("Admin/[controller]")]
public class WishlistsController : AdminControllerBase
{
    private readonly ClothingStoreDbContext _db;

    public WishlistsController(ClothingStoreDbContext db) => _db = db;

    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        var query = _db.Wishlists.AsNoTracking().AsQueryable();

        // Nếu có từ khóa tìm kiếm, lọc theo mã khách hàng, mã sản phẩm hoặc thông tin liên quan
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(w => w.CustomerId.Contains(q) || w.ProductId.Contains(q));
        }

        var pagedList = await PagedList<Wishlist>.CreateAsync(query.OrderByDescending(w => w.AddedDate), page);

        // Nạp thông tin chi tiết Customer và Product an toàn tuyệt đối không gây lỗi shadow property
        foreach (var item in pagedList.Items)
        {
            // Tìm khách hàng khớp theo CustomerId HOẶC Username
            item.Customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CustomerId == item.CustomerId || c.Username == item.CustomerId);

            // Tìm sản phẩm kèm danh mục
            item.Product = await _db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
        }

        // Dữ liệu phục vụ phần thống kê theo danh mục và sản phẩm
        ViewBag.Categories = await _db.Categories.AsNoTracking().ToListAsync();
        ViewBag.AllProducts = await _db.Products.AsNoTracking().ToListAsync();

        ViewBag.ProductWishlistCounts = await _db.Wishlists
            .GroupBy(w => w.ProductId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        ViewBag.Q = q;
        return View(pagedList);
    }

    [HttpPost("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string customerId, string productId)
    {
        var w = await _db.Wishlists.FindAsync(customerId, productId);
        if (w == null) return NotFound();

        _db.Wishlists.Remove(w);
        await _db.SaveChangesAsync();

        Success("Đã xóa khỏi danh sách yêu thích.");
        return RedirectToAction(nameof(Index));
    }
}