using ClothingStoreWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ClothingStoreWeb.Controllers
{
    public class FavoriteController : Controller
    {
        private readonly ClothingStoreDbContext _context;
        private const string CookieKey = "FavoriteProductIds";

        public FavoriteController(ClothingStoreDbContext context)
        {
            _context = context;
        }

        private List<string> GetFavoriteIds()
        {
            var cookie = Request.Cookies[CookieKey];
            if (string.IsNullOrEmpty(cookie)) return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(cookie) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private void SaveFavoriteIds(List<string> ids)
        {
            var options = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                IsEssential = true
            };
            Response.Cookies.Append(CookieKey, JsonSerializer.Serialize(ids), options);
        }

        public async Task<IActionResult> Index()
        {
            var favIds = GetFavoriteIds();
            var products = new List<Product>();

            if (favIds.Any())
            {
                products = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => favIds.Contains(p.ProductId.ToString()))
                    .ToListAsync();
            }

            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Json(new { success = false });

            id = id.Trim();
            var favIds = GetFavoriteIds();
            bool isAdded;

            if (favIds.Contains(id))
            {
                favIds.Remove(id);
                isAdded = false;
            }
            else
            {
                favIds.Add(id);
                isAdded = true;
            }

            SaveFavoriteIds(favIds);

            // --- ĐỒNG BỘ XUỐNG DATABASE CHO BÊN ADMIN ---
            // Kiểm tra xem khách hàng có đang đăng nhập hệ thống hay không (lấy Username hoặc CustomerId)
            var username = User.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                // Tìm Customer tương ứng với tài khoản đang đăng nhập
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);
                if (customer != null)
                {
                    var existingWishlist = await _context.Wishlists
                        .FirstOrDefaultAsync(w => w.CustomerId == customer.CustomerId && w.ProductId == id);

                    if (isAdded && existingWishlist == null)
                    {
                        // Thêm vào DB
                        _context.Wishlists.Add(new Wishlist
                        {
                            CustomerId = customer.CustomerId,
                            ProductId = id,
                            AddedDate = DateTime.Now
                        });
                    }
                    else if (!isAdded && existingWishlist != null)
                    {
                        // Xóa khỏi DB khi bỏ thích
                        _context.Wishlists.Remove(existingWishlist);
                    }
                    await _context.SaveChangesAsync();
                }
            }

            return Json(new
            {
                success = true,
                isAdded = isAdded,
                wishlistCount = favIds.Count
            });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(string id)
        {
            var favIds = GetFavoriteIds();
            if (!string.IsNullOrWhiteSpace(id) && favIds.Contains(id.Trim()))
            {
                favIds.Remove(id.Trim());
                SaveFavoriteIds(favIds);

                // Xóa cả trong DB nếu đang đăng nhập
                var username = User.Identity?.Name;
                if (!string.IsNullOrEmpty(username))
                {
                    var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Username == username);
                    if (customer != null)
                    {
                        var item = await _context.Wishlists
                            .FirstOrDefaultAsync(w => w.CustomerId == customer.CustomerId && w.ProductId == id.Trim());
                        if (item != null)
                        {
                            _context.Wishlists.Remove(item);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }

            return Json(new
            {
                success = true,
                wishlistCount = favIds.Count
            });
        }

        [HttpGet]
        public IActionResult GetInfo()
        {
            var favIds = GetFavoriteIds();
            return Json(new { count = favIds.Count, ids = favIds });
        }
    }
}