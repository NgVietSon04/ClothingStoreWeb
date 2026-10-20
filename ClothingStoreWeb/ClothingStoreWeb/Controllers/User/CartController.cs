using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace ClothingStoreWeb.Controllers
{
    [Authorize] // Bắt buộc đăng nhập cho toàn bộ Controller Giỏ hàng
    public class CartController : Controller
    {
        private readonly ClothingStoreDbContext _context;

        public CartController(ClothingStoreDbContext context)
        {
            _context = context;
        }

        private string GetCurrentCartId()
        {
            string username = User.Identity!.Name!;
            var cart = _context.Carts.FirstOrDefault(c => c.Username == username);

            if (cart == null)
            {
                string newCartId = "C" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                cart = new Cart
                {
                    CartId = newCartId,
                    Username = username,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            return cart.CartId;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            string cartId = GetCurrentCartId();
            var cart = await _context.Carts
                .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Variant)
                        .ThenInclude(v => v.Product)
                .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Variant)
                        .ThenInclude(v => v.Size)
                .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Variant)
                        .ThenInclude(v => v.Color)
                .FirstOrDefaultAsync(c => c.CartId == cartId);

            return View(cart);
        }
        // POST: /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(string id, int quantity = 1)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
            }

            string cartId = GetCurrentCartId();

            // Tìm biến thể (Variant) mặc định hoặc đầu tiên của sản phẩm này
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.ProductId == id || v.VariantId == id);

            if (variant == null)
            {
                // Nếu bảng ProductVariant lưu ProductId là khóa chính hoặc dạng khác, điều chỉnh lại điều kiện cho phù hợp
                variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .FirstOrDefaultAsync(v => v.ProductId == id);
            }

            if (variant == null)
            {
                return Json(new { success = false, message = "Sản phẩm hoặc phân loại không tồn tại." });
            }

            // Kiểm tra xem sản phẩm này đã có trong CartDetails chưa
            var cartDetail = await _context.CartDetails
                .FirstOrDefaultAsync(cd => cd.CartId == cartId && cd.VariantId == variant.VariantId);

            if (cartDetail != null)
            {
                cartDetail.Quantity += quantity;
            }
            else
            {
                cartDetail = new CartDetail
                {
                    CartId = cartId,
                    VariantId = variant.VariantId,
                    Quantity = quantity
                };
                _context.CartDetails.Add(cartDetail);
            }

            await _context.SaveChangesAsync();

            // Lấy lại tổng số lượng và tổng tiền giỏ hàng
            var allDetails = await _context.CartDetails
                .Include(cd => cd.Variant)
                    .ThenInclude(v => v.Product)
                .Where(cd => cd.CartId == cartId)
                .ToListAsync();

            decimal CalcFinalPrice(ProductVariant? v)
            {
                if (v == null) return 0;
                decimal price = (decimal)(v.UnitPrice ?? (double?)(v.Product?.MinPrice ?? 0m) ?? 0d);
                decimal percent = (decimal)(v.Discount ?? 0);
                return percent > 0 ? Math.Round(price * (1 - percent / 100m), 0) : price;
            }

            int cartCount = allDetails.Sum(cd => cd.Quantity);
            decimal cartTotal = allDetails.Sum(cd => cd.Quantity * CalcFinalPrice(cd.Variant));

            return Json(new
            {
                success = true,
                cartCount = cartCount,
                cartTotal = cartTotal.ToString("N0") + " đ"
            });
        }
        // POST: /Cart/UpdateQuantityAjax
        [HttpPost]
        public async Task<IActionResult> UpdateQuantityAjax(string variantId, int quantity)
        {
            string cartId = GetCurrentCartId();
            var cartDetail = await _context.CartDetails
                .Include(cd => cd.Variant)
                    .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(cd => cd.CartId == cartId && cd.VariantId == variantId);

            if (cartDetail == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng." });
            }

            if (quantity <= 0)
            {
                _context.CartDetails.Remove(cartDetail);
            }
            else
            {
                cartDetail.Quantity = quantity;
            }

            await _context.SaveChangesAsync();

            var allDetails = await _context.CartDetails
                .Include(cd => cd.Variant)
                    .ThenInclude(v => v.Product)
                .Where(cd => cd.CartId == cartId)
                .ToListAsync();

            decimal CalcFinalPrice(ProductVariant? v)
            {
                if (v == null) return 0;
                decimal price = (decimal)(v.UnitPrice ?? (double?)(v.Product?.MinPrice ?? 0m) ?? 0d);
                decimal percent = (decimal)(v.Discount ?? 0);
                return percent > 0 ? Math.Round(price * (1 - percent / 100m), 0) : price;
            }

            decimal currentItemFinalPrice = CalcFinalPrice(cartDetail.Variant);
            decimal itemTotal = quantity > 0 ? currentItemFinalPrice * quantity : 0;
            decimal subTotal = allDetails.Sum(cd => cd.Quantity * CalcFinalPrice(cd.Variant));

            return Json(new
            {
                success = true,
                itemTotal = itemTotal.ToString("N0"),
                subTotal = subTotal.ToString("N0"),
                cartCount = allDetails.Sum(cd => cd.Quantity),
                isRemoved = quantity <= 0
            });
        }
        // POST: /Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(string variantId)
        {
            string cartId = GetCurrentCartId();
            var cartDetail = await _context.CartDetails
                .FirstOrDefaultAsync(cd => cd.CartId == cartId && cd.VariantId == variantId);

            if (cartDetail != null)
            {
                _context.CartDetails.Remove(cartDetail);
                await _context.SaveChangesAsync();
            }

            // Nếu là gọi từ Ajax thì trả về Json, nếu gọi thông thường thì Redirect về Index
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var allDetails = await _context.CartDetails
                    .Include(cd => cd.Variant)
                        .ThenInclude(v => v.Product)
                    .Where(cd => cd.CartId == cartId)
                    .ToListAsync();

                decimal CalcFinalPrice(ProductVariant? v)
                {
                    if (v == null) return 0;
                    decimal price = (decimal)(v.UnitPrice ?? (double?)(v.Product?.MinPrice ?? 0m) ?? 0d);
                    decimal percent = (decimal)(v.Discount ?? 0);
                    return percent > 0 ? Math.Round(price * (1 - percent / 100m), 0) : price;
                }

                decimal subTotal = allDetails.Sum(cd => cd.Quantity * CalcFinalPrice(cd.Variant));

                return Json(new
                {
                    success = true,
                    cartCount = allDetails.Sum(cd => cd.Quantity),
                    subTotal = subTotal.ToString("N0") + " đ"
                });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}