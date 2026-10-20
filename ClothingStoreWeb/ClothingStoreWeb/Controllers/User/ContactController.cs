using ClothingStoreWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClothingStoreWeb.Controllers
{
    public class ContactController : Controller
    {
        private readonly ClothingStoreDbContext _context;

        public ContactController(ClothingStoreDbContext context)
        {
            _context = context;
        }

        // Hàm hỗ trợ nạp giỏ hàng dùng chung cho Header
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

            ViewBag.CartCount = 0;
            ViewBag.CartTotal = 0m;
        }

        // GET: /Contact
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await LoadCartSummaryAsync();
            return View();
        }

        // POST: /Contact
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string name, string email, string message)
        {
            await LoadCartSummaryAsync();

            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(message))
            {
                TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi trong thời gian sớm nhất.";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Vui lòng điền đầy đủ họ tên, email và nội dung lời nhắn.";
            return View();
        }
    }
}