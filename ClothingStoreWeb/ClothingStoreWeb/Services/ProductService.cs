using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Services;

public interface IProductService
{
    /// <summary>Cập nhật MinPrice / MaxPrice của sản phẩm theo giá các biến thể.</summary>
    Task RecalcPriceRangeAsync(string? productId);
}

public class ProductService : IProductService
{
    private readonly ClothingStoreDbContext _db;

    public ProductService(ClothingStoreDbContext db) => _db = db;

    public async Task RecalcPriceRangeAsync(string? productId)
    {
        if (string.IsNullOrEmpty(productId)) return;

        var prices = await _db.ProductVariants
            .Where(v => v.ProductId == productId && v.UnitPrice != null)
            .Select(v => v.UnitPrice!.Value)
            .ToListAsync();

        var product = await _db.Products.FindAsync(productId);
        if (product == null) return;

        if (prices.Count == 0)
        {
            product.MinPrice = null;
            product.MaxPrice = null;
        }
        else
        {
            product.MinPrice = (decimal)prices.Min();
            product.MaxPrice = (decimal)prices.Max();
        }
        await _db.SaveChangesAsync();
    }
}
