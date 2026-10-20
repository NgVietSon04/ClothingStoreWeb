using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Controllers
{
    public class BlogController : Controller
    {
        private readonly ClothingStoreDbContext _context;

        public BlogController(ClothingStoreDbContext context)
        {
            _context = context;
        }

        // GET: /Blog hoặc /Blog/Index?page=1
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 6; // Số bài viết hiển thị trên 1 trang
            if (page < 1) page = 1;

            // 1. Lấy tổng số lượng bài viết
            int totalBlogs = await _context.Blogs.CountAsync();

            // 2. Tính tổng số trang (Ví dụ: 13 bài / 6 = 3 trang)
            int totalPages = (int)Math.Ceiling((double)totalBlogs / pageSize);

            // Xử lý nếu tham số page truyền vào vượt quá số trang hiện có
            if (page > totalPages && totalPages > 0) page = totalPages;

            // 3. Lấy đúng 6 bài viết cho trang hiện tại
            var blogs = await _context.Blogs
                .OrderByDescending(b => b.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 4. Truyền thông tin phân trang sang View qua ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(blogs);
        }
    }
}