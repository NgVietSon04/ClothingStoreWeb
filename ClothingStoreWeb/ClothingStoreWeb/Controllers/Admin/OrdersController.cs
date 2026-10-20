using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.Services;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>CRUD đơn hàng + chi tiết đơn, tự trừ / hoàn tồn kho (/Admin/Orders).</summary>
[Route("Admin/[controller]")]
public class OrdersController : AdminControllerBase
{
    private readonly ClothingStoreDbContext _db;
    private readonly ILookupService _lookup;

    public OrdersController(ClothingStoreDbContext db, ILookupService lookup)
    {
        _db = db;
        _lookup = lookup;
    }

    private async Task LoadForm()
    {
        ViewBag.Customers = await _lookup.CustomersAsync();
        ViewBag.Employees = await _lookup.EmployeesAsync();
        ViewBag.Variants = await _lookup.VariantPicksAsync();
    }

    private static string? Nz(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    // ------------------------------------------------------------ DANH SÁCH
    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, string? payment, DateTime? fromDate, DateTime? toDate, int page = 1)
    {
        var query = _db.Orders.AsNoTracking()
            .Include(o => o.Customer)
            .Include(o => o.Employee)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(o => o.OrderId.Contains(q)
                                     || (o.CustomerId != null && o.CustomerId.Contains(q))
                                     || (o.Customer != null && o.Customer.FullName != null && o.Customer.FullName.Contains(q)));
        }
        if (!string.IsNullOrEmpty(payment) && byte.TryParse(payment, out var pm))
            query = query.Where(o => o.PaymentMethod == pm);
        if (fromDate.HasValue)
        {
            var start = fromDate.Value.Date;
            query = query.Where(o => o.OrderDate >= start);
        }
        if (toDate.HasValue)
        {
            var end = toDate.Value.Date.AddDays(1);
            query = query.Where(o => o.OrderDate < end);
        }

        var model = await PagedList<Order>.CreateAsync(query.OrderByDescending(o => o.OrderDate), page);
        ViewBag.Q = q;
        ViewBag.Payment = payment;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        return View(model);
    }

    // ------------------------------------------------------------ CHI TIẾT
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(string id)
    {
        var o = await _db.Orders.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Employee)
            .Include(x => x.OrderDetails).ThenInclude(d => d.Variant).ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(x => x.OrderId == id);
        if (o == null) return NotFound();
        return View(o);
    }

    // ------------------------------------------------------------ THÊM
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await LoadForm();
        var vm = new OrderFormVm { EmployeeId = CurrentEmployeeId };
        vm.Items.Add(new OrderItemVm());
        return View("Form", vm);
    }

    [HttpPost("Create"), ActionName("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(OrderFormVm vm)
    {
        Normalize(vm);
        var variants = await LoadVariants(vm.Items.Select(i => i.VariantId!));
        Validate(vm, variants, new Dictionary<string, int>());

        if (!ModelState.IsValid)
        {
            await LoadForm();
            if (vm.Items.Count == 0) vm.Items.Add(new OrderItemVm());
            return View("Form", vm);
        }

        var order = new Order
        {
            OrderId = NextId(await _db.Orders.Select(o => o.OrderId).ToListAsync(), "HD", 4),
            OrderDate = vm.OrderDate,
            CustomerId = Nz(vm.CustomerId),
            EmployeeId = Nz(vm.EmployeeId) ?? CurrentEmployeeId,
            PaymentMethod = vm.PaymentMethod,
            OrderDiscount = vm.OrderDiscount,
            TaxCode = Nz(vm.TaxCode),
            TaxInfo = Nz(vm.TaxInfo),
            Notes = Nz(vm.Notes),
            TotalAmount = CalcTotal(vm.Items, vm.OrderDiscount)
        };

        foreach (var it in vm.Items)
        {
            var v = variants[it.VariantId!];
            v.StockQuantity = (v.StockQuantity ?? 0) - it.Quantity;
            order.OrderDetails.Add(new OrderDetail
            {
                VariantId = it.VariantId!,
                Quantity = it.Quantity,
                UnitPrice = it.UnitPrice,
                Discount = it.Discount,
                Notes = Nz(it.Notes)
            });
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        Success($"Đã tạo đơn hàng {order.OrderId}.");
        return RedirectToAction(nameof(Details), new { id = order.OrderId });
    }

    // ------------------------------------------------------------ SỬA
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        var o = await _db.Orders.AsNoTracking().Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.OrderId == id);
        if (o == null) return NotFound();

        var vm = new OrderFormVm
        {
            OrderId = o.OrderId,
            OrderDate = o.OrderDate ?? DateTime.Now,
            CustomerId = o.CustomerId,
            EmployeeId = o.EmployeeId,
            PaymentMethod = o.PaymentMethod ?? 0,
            OrderDiscount = o.OrderDiscount ?? 0,
            TaxCode = o.TaxCode,
            TaxInfo = o.TaxInfo,
            Notes = o.Notes,
            Items = o.OrderDetails.Select(d => new OrderItemVm
            {
                VariantId = d.VariantId,
                Quantity = d.Quantity ?? 0,
                UnitPrice = d.UnitPrice ?? 0,
                Discount = d.Discount ?? 0,
                Notes = d.Notes
            }).ToList()
        };
        await LoadForm();
        return View("Form", vm);
    }

    [HttpPost("Edit/{id}"), ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(string id, OrderFormVm vm)
    {
        var order = await _db.Orders.Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.OrderId == id);
        if (order == null) return NotFound();
        vm.OrderId = id;

        Normalize(vm);
        var oldQty = order.OrderDetails.ToDictionary(d => d.VariantId, d => d.Quantity ?? 0);
        var variants = await LoadVariants(vm.Items.Select(i => i.VariantId!).Concat(oldQty.Keys));
        Validate(vm, variants, oldQty);

        if (!ModelState.IsValid)
        {
            await LoadForm();
            if (vm.Items.Count == 0) vm.Items.Add(new OrderItemVm());
            return View("Form", vm);
        }

        // 1) hoàn lại tồn kho của chi tiết cũ
        foreach (var d in order.OrderDetails)
        {
            if (variants.TryGetValue(d.VariantId, out var ov))
                ov.StockQuantity = (ov.StockQuantity ?? 0) + (d.Quantity ?? 0);
        }

        // 2) đồng bộ chi tiết: sửa dòng còn lại, xóa dòng bỏ đi, thêm dòng mới
        var newIds = vm.Items.Select(i => i.VariantId!).ToHashSet();
        foreach (var d in order.OrderDetails.Where(d => !newIds.Contains(d.VariantId)).ToList())
            _db.OrderDetails.Remove(d);

        foreach (var it in vm.Items)
        {
            var v = variants[it.VariantId!];
            v.StockQuantity = (v.StockQuantity ?? 0) - it.Quantity;

            var existing = order.OrderDetails.FirstOrDefault(d => d.VariantId == it.VariantId);
            if (existing != null)
            {
                existing.Quantity = it.Quantity;
                existing.UnitPrice = it.UnitPrice;
                existing.Discount = it.Discount;
                existing.Notes = Nz(it.Notes);
            }
            else
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    VariantId = it.VariantId!,
                    Quantity = it.Quantity,
                    UnitPrice = it.UnitPrice,
                    Discount = it.Discount,
                    Notes = Nz(it.Notes)
                });
            }
        }

        order.OrderDate = vm.OrderDate;
        order.CustomerId = Nz(vm.CustomerId);
        order.EmployeeId = Nz(vm.EmployeeId);
        order.PaymentMethod = vm.PaymentMethod;
        order.OrderDiscount = vm.OrderDiscount;
        order.TaxCode = Nz(vm.TaxCode);
        order.TaxInfo = Nz(vm.TaxInfo);
        order.Notes = Nz(vm.Notes);
        order.TotalAmount = CalcTotal(vm.Items, vm.OrderDiscount);

        await _db.SaveChangesAsync();
        Success($"Đã cập nhật đơn hàng {id}.");
        return RedirectToAction(nameof(Details), new { id });
    }

    // ------------------------------------------------------------ XÓA
    [HttpPost("Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var order = await _db.Orders.Include(x => x.OrderDetails).FirstOrDefaultAsync(x => x.OrderId == id);
        if (order == null) return NotFound();

        var variants = await LoadVariants(order.OrderDetails.Select(d => d.VariantId));
        foreach (var d in order.OrderDetails)
        {
            if (variants.TryGetValue(d.VariantId, out var v))
                v.StockQuantity = (v.StockQuantity ?? 0) + (d.Quantity ?? 0);
        }

        _db.OrderDetails.RemoveRange(order.OrderDetails);
        _db.Orders.Remove(order);
        await _db.SaveChangesAsync();

        Success($"Đã xóa đơn hàng {id} và hoàn lại tồn kho.");
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------ HELPER
    private static void Normalize(OrderFormVm vm)
    {
        vm.Items = (vm.Items ?? new List<OrderItemVm>())
            .Where(i => !string.IsNullOrWhiteSpace(i.VariantId))
            .GroupBy(i => i.VariantId!.Trim())
            .Select(g => new OrderItemVm
            {
                VariantId = g.Key,
                Quantity = g.Sum(x => x.Quantity),
                UnitPrice = g.First().UnitPrice,
                Discount = g.First().Discount,
                Notes = g.First().Notes
            }).ToList();
    }

    private async Task<Dictionary<string, ProductVariant>> LoadVariants(IEnumerable<string> ids)
    {
        var list = ids.Distinct().ToList();
        return await _db.ProductVariants.Where(v => list.Contains(v.VariantId)).ToDictionaryAsync(v => v.VariantId);
    }

    /// <param name="oldQty">Số lượng cũ theo biến thể (khi sửa đơn) để tính tồn kho khả dụng.</param>
    private void Validate(OrderFormVm vm, Dictionary<string, ProductVariant> variants, Dictionary<string, int> oldQty)
    {
        if (vm.Items.Count == 0)
            ModelState.AddModelError("", "Đơn hàng cần ít nhất 1 sản phẩm.");
        if (vm.OrderDiscount < 0 || vm.OrderDiscount > 100)
            ModelState.AddModelError("", "Giảm giá đơn phải từ 0 đến 100 (%).");

        foreach (var it in vm.Items)
        {
            if (!variants.TryGetValue(it.VariantId!, out var v))
            {
                ModelState.AddModelError("", $"Biến thể '{it.VariantId}' không tồn tại.");
                continue;
            }
            if (it.Quantity <= 0)
                ModelState.AddModelError("", $"Số lượng của {it.VariantId} phải lớn hơn 0.");
            if (it.UnitPrice < 0)
                ModelState.AddModelError("", $"Đơn giá của {it.VariantId} không được âm.");
            if (it.Discount < 0 || it.Discount > 100)
                ModelState.AddModelError("", $"Giảm giá của {it.VariantId} phải từ 0 đến 100 (%).");

            var available = (v.StockQuantity ?? 0) + (oldQty.TryGetValue(it.VariantId!, out var q) ? q : 0);
            if (it.Quantity > available)
                ModelState.AddModelError("", $"{it.VariantId} chỉ còn {available} trong kho, không đủ {it.Quantity}.");
        }
    }

    /// <summary>Tổng = Σ(SL × đơn giá × (1 − giảm giá dòng%)) × (1 − giảm giá đơn%), làm tròn đến đồng.</summary>
    private static decimal CalcTotal(IEnumerable<OrderItemVm> items, double orderDiscount)
    {
        var sub = items.Sum(i => i.Quantity * i.UnitPrice * (decimal)(1 - i.Discount / 100.0));
        return Math.Round(sub * (decimal)(1 - orderDiscount / 100.0), 0);
    }
}
