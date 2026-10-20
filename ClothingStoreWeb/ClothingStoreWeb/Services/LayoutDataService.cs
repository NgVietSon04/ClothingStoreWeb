using ClothingStoreWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothingStoreWeb.Services;

/// <summary>Thông tin cửa hàng, đọc từ appsettings.json mục "Shop".</summary>
public class ShopSettings
{
    public const string SectionName = "Shop";

    public string Name { get; set; } = "Clothing Store";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public decimal FreeShipThreshold { get; set; } = 500000;
    public int FirstOrderDiscountPercent { get; set; } = 10;
    public string Facebook { get; set; } = "#";
    public string Instagram { get; set; } = "#";
    public string TikTok { get; set; } = "#";
}

/// <summary>Dữ liệu dùng chung cho mọi trang (header, menu, hero, ...).</summary>
public class LayoutData
{
    public bool IsAuthenticated { get; set; }
    public string? UserName { get; set; }
    public int CartCount { get; set; }
    public decimal CartTotal { get; set; }
    public int WishlistCount { get; set; }
    public List<Category> Categories { get; set; } = new();
}

public interface ILayoutDataService
{
    Task<LayoutData> GetAsync();
}

public class LayoutDataService : ILayoutDataService
{
    private readonly ClothingStoreDbContext _db;
    private readonly IHttpContextAccessor _http;

    public LayoutDataService(ClothingStoreDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task<LayoutData> GetAsync()
    {
        var user = _http.HttpContext?.User;
        var data = new LayoutData
        {
            IsAuthenticated = user?.Identity?.IsAuthenticated == true,
            UserName = user?.Identity?.Name
        };

        data.Categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .ToListAsync();

        // Khách chưa đăng nhập: giỏ hàng và yêu thích = 0
        // (bảng Cart đang gắn với Username nên chưa hỗ trợ giỏ của khách vãng lai)
        if (!data.IsAuthenticated || string.IsNullOrEmpty(data.UserName))
            return data;

        var username = data.UserName;

        // Giỏ hàng mới nhất của user
        var cartId = await _db.Carts
            .AsNoTracking()
            .Where(c => c.Username == username)
            .OrderByDescending(c => c.UpdatedDate ?? c.CreatedDate)
            .Select(c => c.CartId)
            .FirstOrDefaultAsync();

        if (cartId != null)
        {
            var lines = await _db.CartDetails
                .AsNoTracking()
                .Where(d => d.CartId == cartId)
                .Select(d => new
                {
                    d.Quantity,
                    Price = d.Variant.UnitPrice,
                    Discount = d.Variant.Discount
                })
                .ToListAsync();

            data.CartCount = lines.Sum(l => l.Quantity);
            data.CartTotal = (decimal)lines.Sum(l =>
                l.Quantity * (l.Price ?? 0) * (1 - (l.Discount ?? 0) / 100.0));
        }

        data.WishlistCount = await _db.Wishlists
            .AsNoTracking()
            .CountAsync(w => w.Customer.Username == username);

        return data;
    }
}