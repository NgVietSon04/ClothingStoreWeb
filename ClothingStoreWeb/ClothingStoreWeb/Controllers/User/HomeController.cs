using System.Diagnostics;
using ClothingStoreWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingStoreWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ClothingStoreDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ClothingStoreDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Hàm hỗ trợ nạp giỏ hàng dùng chung cho Header (lọc theo user nếu đã đăng nhập)
        private async Task LoadCartSummaryAsync()
        {
            var query = _context.CartDetails
                .Include(cd => cd.Variant)
                    .ThenInclude(v => v!.Product)
                .AsNoTracking();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var username = User.Identity.Name;
                var cart = await _context.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.Username == username);
                if (cart != null)
                {
                    var userCartDetails = await query.Where(cd => cd.CartId == cart.CartId).ToListAsync();
                    ViewBag.CartCount = userCartDetails.Sum(x => x.Quantity);
                    ViewBag.CartTotal = userCartDetails.Sum(x => x.Quantity * (x.Variant?.Product?.MinPrice ?? 0));
                    return;
                }
            }

            // Mặc định nếu chưa đăng nhập hoặc chưa có giỏ
            ViewBag.CartCount = 0;
            ViewBag.CartTotal = 0m;
        }

        public async Task<IActionResult> Index()
        {
            // 0. Tải dữ liệu giỏ hàng lên ViewBag cho Header
            await LoadCartSummaryAsync();

            // 1. Lấy danh sách Category (Danh mục trượt ngang)
            ViewBag.Categories = await _context.Categories.AsNoTracking().ToListAsync();

            // 2. Lấy toàn bộ danh sách sản phẩm
            var listProducts = await _context.Products.AsNoTracking().ToListAsync();

            // 3. Lấy TỐI THIỂU 6 SẢN PHẨM cho mỗi khối
            ViewBag.LatestProducts = await _context.Products.AsNoTracking()
                .OrderByDescending(p => p.ProductId)
                .Take(6)
                .ToListAsync();

            ViewBag.TopRatedProducts = await _context.Products.AsNoTracking()
                .Take(6)
                .ToListAsync();

            ViewBag.ReviewProducts = await _context.Products.AsNoTracking()
                .OrderByDescending(p => p.MinPrice)
                .Take(6)
                .ToListAsync();

            // 4. Lấy danh sách bài viết blog
            if (_context.Blogs != null)
            {
                ViewBag.Blogs = await _context.Blogs.AsNoTracking()
                    .OrderByDescending(b => b.CreatedDate)
                    .Take(6)
                    .ToListAsync();
            }

            return View(listProducts);
        }

        /// <summary>
        /// Hàm chuyển hướng trung gian: Nhận yêu cầu ProductDetail từ trang chủ hoặc các view cũ,
        /// sau đó tự động điều hướng sang trang chi tiết chuẩn nằm trong ShopController.
        /// </summary>
        public IActionResult ProductDetail(string id)
        {
            return RedirectToAction("Details", "Shop", new { id = id });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}