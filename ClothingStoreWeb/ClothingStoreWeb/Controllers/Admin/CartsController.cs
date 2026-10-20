using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
// using ClothingStoreWeb.Models.Admin; // Bỏ comment hoặc điều chỉnh nếu namespace của AdminControllerBase khác

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>Xem / xóa giỏ hàng của khách (/Admin/Carts).</summary>
[Route("Admin/[controller]")]
public class CartsController : AdminControllerBase
{
    private readonly ClothingStoreDbContext _db;

    public CartsController(ClothingStoreDbContext db) => _db = db;

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, int page = 1)
    {
        // Đổi c.Details thành c.CartDetails
        var query = _db.Carts.AsNoTracking().Include(c => c.CartDetails).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(c => c.CartId.Contains(q) || (c.Username != null && c.Username.Contains(q)));
        }
        ViewBag.Q = q;
        return View(await PagedList<Cart>.CreateAsync(query.OrderByDescending(c => c.UpdatedDate), page));
    }

    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(string id)
    {
        // Đổi c.Details thành c.CartDetails
        var cart = await _db.Carts.AsNoTracking()
            .Include(c => c.CartDetails).ThenInclude(d => d.Variant).ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(c => c.CartId == id);
        if (cart == null) return NotFound();
        return View(cart);
    }

    [HttpPost("Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        // Đổi c.Details thành c.CartDetails
        var cart = await _db.Carts.Include(c => c.CartDetails).FirstOrDefaultAsync(c => c.CartId == id);
        if (cart == null) return NotFound();

        _db.CartDetails.RemoveRange(cart.CartDetails);
        _db.Carts.Remove(cart);
        await _db.SaveChangesAsync();

        Success("Đã xóa giỏ hàng.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("DeleteItem"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(string cartId, string variantId)
    {
        var item = await _db.CartDetails.FindAsync(cartId, variantId);
        if (item == null) return NotFound();

        _db.CartDetails.Remove(item);
        await _db.SaveChangesAsync();

        Success("Đã xóa sản phẩm khỏi giỏ.");
        return RedirectToAction(nameof(Details), new { id = cartId });
    }
}