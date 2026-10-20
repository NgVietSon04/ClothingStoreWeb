using ClothingStoreWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingStoreWeb.Controllers
{
    public class ShopController : Controller
    {
        private readonly ClothingStoreDbContext _context;

        public ShopController(ClothingStoreDbContext context)
        {
            _context = context;
        }

        // Hàm hỗ trợ tính toán giỏ hàng dùng chung
        private async Task LoadCartSummaryAsync()
        {
            // Truy vấn lấy chi tiết giỏ hàng kèm Variant để tính tổng tiền
            var cartDetails = await _context.CartDetails
                .Include(cd => cd.Variant)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.CartCount = cartDetails.Sum(x => x.Quantity);
            ViewBag.CartTotal = cartDetails.Sum(x => x.Quantity * (x.Variant?.Product?.MinPrice ?? 0));
        }

        // GET: /Shop hoặc /Shop/Index
        public async Task<IActionResult> Index(string? categoryId, string? query, decimal? minPrice, decimal? maxPrice, int sort = 0, int page = 1)
        {
            int pageSize = 9; // Hiển thị 9 sản phẩm trên 1 trang (3 hàng x 3 cột)

            // 0. Tải dữ liệu giỏ hàng lên ViewBag cho Header
            await LoadCartSummaryAsync();

            // 1. Lấy danh sách Danh mục cho cột Sidebar bên trái
            ViewBag.Categories = await _context.Categories.AsNoTracking().ToListAsync();

            // 2. Lấy 6 sản phẩm mới nhất cho slider "Latest Products" ở Sidebar
            ViewBag.LatestProducts = await _context.Products.AsNoTracking()
                .OrderByDescending(p => p.ProductId)
                .Take(6)
                .ToListAsync();

            // 3. Lấy 6 sản phẩm cho phần "Sale Off" (Giảm giá)
            ViewBag.DiscountProducts = await _context.Products.AsNoTracking()
                .Include(p => p.Category)
                .OrderBy(p => p.MinPrice)
                .Take(6)
                .ToListAsync();

            // 4. Truy vấn danh sách sản phẩm chính (có lọc theo điều kiện)
            var productsQuery = _context.Products.AsNoTracking().AsQueryable();

            // Lọc theo danh mục (CategoryId)
            if (!string.IsNullOrEmpty(categoryId))
            {
                productsQuery = productsQuery.Where(p => p.CategoryId.ToString() == categoryId);
            }

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(query))
            {
                productsQuery = productsQuery.Where(p => p.ProductName != null && p.ProductName.Contains(query));
            }

            // Lọc theo khoảng giá
            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.MinPrice >= minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.MinPrice <= maxPrice.Value);
            }

            // Sắp xếp
            productsQuery = sort switch
            {
                1 => productsQuery.OrderBy(p => p.MinPrice), // Giá tăng dần
                2 => productsQuery.OrderByDescending(p => p.MinPrice), // Giá giảm dần
                _ => productsQuery.OrderByDescending(p => p.ProductId) // Mặc định: Mới nhất
            };

            // 5. Tính toán phân trang
            int totalItems = await productsQuery.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (page < 1) page = 1;

            var pagedProducts = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lưu các biến trạng thái xuống View
            ViewBag.CurrentCategory = categoryId;
            ViewBag.SearchQuery = query;
            ViewBag.Sort = sort;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalProducts = totalItems;

            return View(pagedProducts);
        }

        // Action nhận từ khóa từ thanh tìm kiếm trên Header/Hero
        [HttpGet]
        public IActionResult Search(string? query)
        {
            return RedirectToAction(nameof(Index), new { query = query });
        }

        // GET: /Shop/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            // 0. Tải dữ liệu giỏ hàng lên ViewBag cho Header
            await LoadCartSummaryAsync();

            // 1. Lấy thông tin sản phẩm kèm tên Danh mục
            var product = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProductId.ToString() == id);

            if (product == null)
            {
                return NotFound();
            }

            // 2. Lấy 4 sản phẩm liên quan (cùng CategoryId nhưng khác sản phẩm hiện tại)
            ViewBag.RelatedProducts = await _context.Products
                .AsNoTracking()
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                .Take(4)
                .ToListAsync();

            return View(product);
        }
    }
}