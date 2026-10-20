using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới xem được lịch sử/tra cứu đơn hàng
    public class OrderController : Controller
    {
        private readonly ClothingStoreDbContext _db;

        public OrderController(ClothingStoreDbContext db)
        {
            _db = db;
        }

        // GET: /Order/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var username = User.Identity!.Name!;

            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null)
            {
                return RedirectToAction("LoginRegister", "Account");
            }

            var orders = await _db.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                        .ThenInclude(v => v.Product)
                .Where(o => o.CustomerId == customer.CustomerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders); // Trỏ tới file Views/Order/History.cshtml (hoặc dùng chung Views/Checkout/History.cshtml)
        }

        // GET: /Order/TrackOrder
        [HttpGet]
        public async Task<IActionResult> TrackOrder(string? orderCode)
        {
            var username = User.Identity!.Name!;
            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null)
            {
                return RedirectToAction("LoginRegister", "Account");
            }

            var query = _db.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                        .ThenInclude(v => v.Product)
                .Where(o => o.CustomerId == customer.CustomerId);

            // Nếu người dùng nhập mã đơn để tìm kiếm
            if (!string.IsNullOrEmpty(orderCode))
            {
                query = query.Where(o => o.OrderId.Contains(orderCode));
                ViewBag.SearchCode = orderCode;
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            return View(orders);
        }

        // POST: /Order/CancelOrder
        [HttpPost]
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
            {
                TempData["Error"] = "Mã đơn hàng không hợp lệ.";
                return RedirectToAction("History");
            }

            var username = User.Identity!.Name!;
            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Username == username);

            if (customer == null)
            {
                return RedirectToAction("LoginRegister", "Account");
            }

            // Lấy đơn hàng thuộc về khách hàng này
            var order = await _db.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customer.CustomerId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("History");
            }

            // Kiểm tra điều kiện được phép hủy (Ví dụ: Status == 0 hoặc chưa đi giao)
            // Giả sử cột Status của bạn là byte? hoặc int?
            // Nếu bạn dùng cột Status: if (order.Status != 0) { ... báo lỗi ... }

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1. Hoàn lại số lượng tồn kho sản phẩm
                foreach (var detail in order.OrderDetails)
                {
                    if (detail.Variant != null)
                    {
                        detail.Variant.StockQuantity += detail.Quantity;
                        _db.ProductVariants.Update(detail.Variant);
                    }
                }

                // 2. Cập nhật trạng thái đơn hàng thành Đã hủy (Ví dụ: Status = 3 hoặc xóa đơn, ở đây khuyến nghị cập nhật Status = 3 thay vì xóa hẳn để lưu vết)
                // order.Status = 3; // Hoặc nếu bạn xóa hẳn: _db.Orders.Remove(order);

                // Nếu dự án của bạn dùng xóa mềm hoặc cập nhật cột Status, hãy điều chỉnh tương ứng. 
                // Dưới đây là ví dụ cập nhật trạng thái hủy (giả sử Status = 3 là Đã hủy):
                // order.Status = 3;

                // Hoặc nếu bạn muốn xóa đơn hàng khỏi lịch sử (tùy nghiệp vụ):
                _db.OrderDetails.RemoveRange(order.OrderDetails);
                _db.Orders.Remove(order);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"Đã hủy thành công đơn hàng #{orderId}.";
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "Có lỗi xảy ra khi hủy đơn hàng. Vui lòng thử lại.";
            }

            return RedirectToAction("History");
        }


    }
}