using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ClothingStoreWeb.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ClothingStoreDbContext _db;
        private readonly ShopSettings _shop;

        private const decimal DefaultShippingFee = 30000m;

        public CheckoutController(ClothingStoreDbContext db, IOptions<ShopSettings> shopOptions)
        {
            _db = db;
            _shop = shopOptions.Value;
        }

        private string GetCurrentCartId(string username)
        {
            var cart = _db.Carts.FirstOrDefault(c => c.Username == username);
            if (cart == null)
            {
                cart = new Cart
                {
                    CartId = "C" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    Username = username,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };
                _db.Carts.Add(cart);
                _db.SaveChanges();
            }
            return cart.CartId;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? couponCode = null)
        {
            var username = User.Identity!.Name!;
            var cartId = GetCurrentCartId(username);

            var cartDetails = await _db.CartDetails
                .AsNoTracking()
                .Include(cd => cd.Variant).ThenInclude(v => v!.Product)
                .Include(cd => cd.Variant).ThenInclude(v => v!.Size)
                .Include(cd => cd.Variant).ThenInclude(v => v!.Color)
                .Where(cd => cd.CartId == cartId)
                .ToListAsync();

            var outOfStock = cartDetails.FirstOrDefault(cd => cd.Quantity > (cd.Variant?.StockQuantity ?? 0));
            if (outOfStock != null)
            {
                TempData["Error"] = $"\"{outOfStock.Variant?.Product?.ProductName}\" chỉ còn {outOfStock.Variant?.StockQuantity ?? 0} sản phẩm, vui lòng cập nhật giỏ hàng.";
                return RedirectToAction("Index", "Cart");
            }

            var customer = await _db.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Username == username);

            var addresses = new List<CustomerAddress>();
            if (customer != null)
            {
                addresses = await _db.CustomerAddresses
                    .AsNoTracking()
                    .Where(a => a.CustomerId == customer.CustomerId)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.AddressId)
                    .ToListAsync();
            }

            ViewBag.Addresses = addresses;
            ViewBag.SelectedAddressId = addresses.Count > 0
                ? (addresses.FirstOrDefault(a => a.IsDefault == true) ?? addresses[0]).AddressId
                : 0;

            ViewBag.PrefillName = customer?.FullName;
            ViewBag.PrefillPhone = customer?.PhoneNumber;
            ViewBag.PrefillAddress = customer?.Address;

            var subtotal = cartDetails.Sum(cd => FinalPrice(cd.Variant!) * cd.Quantity);

            var threshold = (decimal)(_shop?.FreeShipThreshold ?? 0);
            var shippingFee = cartDetails.Count == 0 || (threshold > 0 && subtotal >= threshold)
                ? 0m
                : DefaultShippingFee;

            decimal discount = 0;
            if (!string.IsNullOrWhiteSpace(couponCode) && cartDetails.Count > 0)
            {
                var (d, error) = await CalculateCouponAsync(couponCode.Trim(), subtotal);
                if (error != null)
                {
                    TempData["Error"] = error;
                    couponCode = null;
                }
                else
                {
                    discount = d;
                }
            }

            ViewBag.CouponCode = couponCode;
            ViewBag.Subtotal = subtotal;
            ViewBag.ShippingFee = shippingFee;
            ViewBag.Discount = discount;
            ViewBag.Total = Math.Max(0, subtotal + shippingFee - discount);

            return View(cartDetails);
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(
            int selectedAddressId,
            string? recipientName,
            string? phoneNumber,
            string? addressDetail,
            bool? saveAddress,
            string? note,
            bool? requestInvoice,
            string? taxCode,
            string? taxInfo,
            int paymentMethod)
        {
            var username = User.Identity!.Name!;
            var cartId = GetCurrentCartId(username);

            var cartDetails = await _db.CartDetails
                .Include(cd => cd.Variant)
                    .ThenInclude(v => v!.Product)
                .Where(cd => cd.CartId == cartId)
                .ToListAsync();

            if (cartDetails == null || !cartDetails.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Cart");
            }

            foreach (var cd in cartDetails)
            {
                if (cd.Quantity > (cd.Variant?.StockQuantity ?? 0))
                {
                    TempData["Error"] = $"Sản phẩm \"{cd.Variant?.Product?.ProductName}\" không đủ số lượng trong kho.";
                    return RedirectToAction("Index");
                }
            }

            var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Username == username);
            if (customer == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("Index");
            }

            if (selectedAddressId == 0 && !string.IsNullOrEmpty(addressDetail) && saveAddress == true)
            {
                var newAddr = new CustomerAddress
                {
                    CustomerId = customer.CustomerId,
                    RecipientName = recipientName ?? customer.FullName ?? "",
                    PhoneNumber = phoneNumber ?? customer.PhoneNumber ?? "",
                    AddressDetail = addressDetail,
                    IsDefault = false
                };
                _db.CustomerAddresses.Add(newAddr);
            }

            var subtotal = cartDetails.Sum(cd => FinalPrice(cd.Variant!) * cd.Quantity);
            var threshold = (decimal)(_shop?.FreeShipThreshold ?? 0);
            var shippingFee = (threshold > 0 && subtotal >= threshold) ? 0m : 30000m;

            decimal discount = 0m;
            var totalAmount = Math.Max(0, subtotal + shippingFee - discount);

            string orderId = "HD" + DateTime.Now.ToString("yyyyMMddHHmmss");

            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var order = new Order
                {
                    OrderId = orderId,
                    OrderDate = DateTime.Now,
                    CustomerId = customer.CustomerId,
                    TotalAmount = totalAmount,
                    OrderDiscount = (double)discount,
                    PaymentMethod = (byte)paymentMethod,
                    TaxCode = requestInvoice == true ? taxCode : null,
                    TaxInfo = requestInvoice == true ? taxInfo : null,
                    Notes = note
                };

                _db.Orders.Add(order);
                await _db.SaveChangesAsync();

                foreach (var cd in cartDetails)
                {
                    var unitPrice = FinalPrice(cd.Variant!);
                    var orderDetail = new OrderDetail
                    {
                        OrderId = orderId,
                        VariantId = cd.VariantId,
                        Quantity = cd.Quantity,
                        UnitPrice = unitPrice,
                        Discount = cd.Variant?.Discount ?? 0,
                        Notes = null
                    };
                    _db.OrderDetails.Add(orderDetail);

                    if (cd.Variant != null)
                    {
                        cd.Variant.StockQuantity -= cd.Quantity;
                        _db.ProductVariants.Update(cd.Variant);
                    }
                }

                _db.CartDetails.RemoveRange(cartDetails);

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Đặt hàng thành công!";
                return RedirectToAction("Success", new { id = orderId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                TempData["Error"] = "Lỗi đặt hàng chi tiết: " + errorMsg;

                return RedirectToAction("Index");
            }
        }

        private static decimal FinalPrice(ProductVariant v)
        {
            var price = (decimal)(v.UnitPrice ?? 0);
            var percent = v.Discount ?? 0;
            return percent > 0 ? Math.Round(price * (decimal)(1 - percent / 100d), 0) : price;
        }

        private async Task<(decimal discount, string? error)> CalculateCouponAsync(string code, decimal subtotal)
        {
            var now = DateTime.Now;
            var promo = await _db.Promotions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == code);

            if (promo == null)
                return (0, "Mã giảm giá không tồn tại.");
            if (now < promo.StartDate || now > promo.EndDate)
                return (0, "Mã giảm giá đã hết hạn hoặc chưa đến thời gian áp dụng.");
            if (promo.UsageLimit != null && (promo.UsedCount ?? 0) >= promo.UsageLimit)
                return (0, "Mã giảm giá đã hết lượt sử dụng.");
            if (promo.MinOrderValue != null && subtotal < promo.MinOrderValue)
                return (0, $"Đơn hàng tối thiểu {promo.MinOrderValue:N0}đ để dùng mã này.");

            var discount = promo.DiscountType == 0
                ? subtotal * (decimal)promo.DiscountValue / 100m
                : (decimal)promo.DiscountValue;

            if (promo.DiscountType == 0 && promo.MaxDiscountAmount != null)
                discount = Math.Min(discount, promo.MaxDiscountAmount.Value);

            return (Math.Min(Math.Round(discount, 0), subtotal), null);
        }

        [HttpGet]
        public async Task<IActionResult> Success(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _db.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Variant)
                        .ThenInclude(v => v!.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đơn hàng.";
                return RedirectToAction("Index", "Home");
            }

            return View(order);
        }

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
                        .ThenInclude(v => v!.Product)
                .Where(o => o.CustomerId == customer.CustomerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}