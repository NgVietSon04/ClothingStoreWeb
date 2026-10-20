using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;
using ClothingStoreWeb.Services;

namespace ClothingStoreWeb.Controllers.Admin;

/// <summary>CRUD phiếu nhập kho, tự cộng / hoàn tồn kho (/Admin/PurchaseOrders).</summary>
[Route("Admin/[controller]")]
public class PurchaseOrdersController : AdminControllerBase
{
    private readonly ClothingStoreDbContext _db;
    private readonly ILookupService _lookup;

    public PurchaseOrdersController(ClothingStoreDbContext db, ILookupService lookup)
    {
        _db = db;
        _lookup = lookup;
    }

    private async Task LoadForm()
    {
        ViewBag.Suppliers = await _lookup.SuppliersAsync();
        ViewBag.Employees = await _lookup.EmployeesAsync();
        ViewBag.Variants = await _lookup.VariantPicksAsync();
    }

    private static string? Nz(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    // ------------------------------------------------------------ DANH SÁCH
    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, string? supplierId, DateTime? fromDate, DateTime? toDate, int page = 1)
    {
        var query = _db.PurchaseOrders.AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.Employee)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            query = query.Where(p => p.PurchaseOrderId.Contains(q)
                                     || (p.Supplier != null && p.Supplier.SupplierName.Contains(q)));
        }
        if (!string.IsNullOrEmpty(supplierId)) query = query.Where(p => p.SupplierId == supplierId);
        if (fromDate.HasValue)
        {
            var start = fromDate.Value.Date;
            query = query.Where(p => p.ImportDate >= start);
        }
        if (toDate.HasValue)
        {
            var end = toDate.Value.Date.AddDays(1);
            query = query.Where(p => p.ImportDate < end);
        }

        var model = await PagedList<PurchaseOrder>.CreateAsync(query.OrderByDescending(p => p.ImportDate), page);
        ViewBag.Q = q;
        ViewBag.SupplierId = supplierId;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Suppliers = await _lookup.SuppliersAsync();
        return View(model);
    }

    // ------------------------------------------------------------ CHI TIẾT
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(string id)
    {
        var p = await _db.PurchaseOrders.AsNoTracking()
            .Include(x => x.Supplier)
            .Include(x => x.Employee)
            .Include(x => x.Details).ThenInclude(d => d.Variant).ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(x => x.PurchaseOrderId == id);
        if (p == null) return NotFound();
        return View(p);
    }

    // ------------------------------------------------------------ THÊM
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        await LoadForm();
        var vm = new PurchaseOrderFormVm { EmployeeId = CurrentEmployeeId };
        vm.Items.Add(new PoItemVm());
        return View("Form", vm);
    }

    [HttpPost("Create"), ActionName("Create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePost(PurchaseOrderFormVm vm)
    {
        Normalize(vm);
        var variants = await LoadVariants(vm.Items.Select(i => i.VariantId!));
        Validate(vm, variants, new Dictionary<string, int>());

        if (!ModelState.IsValid)
        {
            await LoadForm();
            if (vm.Items.Count == 0) vm.Items.Add(new PoItemVm());
            return View("Form", vm);
        }

        var po = new PurchaseOrder
        {
            PurchaseOrderId = NextId(await _db.PurchaseOrders.Select(p => p.PurchaseOrderId).ToListAsync(), "PN", 4),
            SupplierId = vm.SupplierId!,
            EmployeeId = Nz(vm.EmployeeId) ?? CurrentEmployeeId,
            ImportDate = vm.ImportDate,
            Notes = Nz(vm.Notes),
            TotalAmount = vm.Items.Sum(i => i.Quantity * i.ImportPrice)
        };

        foreach (var it in vm.Items)
        {
            var v = variants[it.VariantId!];
            v.StockQuantity = (v.StockQuantity ?? 0) + it.Quantity;
            po.Details.Add(new PurchaseOrderDetail
            {
                VariantId = it.VariantId!,
                Quantity = it.Quantity,
                ImportPrice = it.ImportPrice
            });
        }

        _db.PurchaseOrders.Add(po);
        await _db.SaveChangesAsync();

        Success($"Đã tạo phiếu nhập {po.PurchaseOrderId} và cộng tồn kho.");
        return RedirectToAction(nameof(Details), new { id = po.PurchaseOrderId });
    }

    // ------------------------------------------------------------ SỬA
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        var p = await _db.PurchaseOrders.AsNoTracking().Include(x => x.Details).FirstOrDefaultAsync(x => x.PurchaseOrderId == id);
        if (p == null) return NotFound();

        var vm = new PurchaseOrderFormVm
        {
            PurchaseOrderId = p.PurchaseOrderId,
            ImportDate = p.ImportDate ?? DateTime.Now,
            SupplierId = p.SupplierId,
            EmployeeId = p.EmployeeId,
            Notes = p.Notes,
            Items = p.Details.Select(d => new PoItemVm
            {
                VariantId = d.VariantId,
                Quantity = d.Quantity,
                ImportPrice = d.ImportPrice
            }).ToList()
        };
        await LoadForm();
        return View("Form", vm);
    }

    [HttpPost("Edit/{id}"), ActionName("Edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(string id, PurchaseOrderFormVm vm)
    {
        var po = await _db.PurchaseOrders.Include(x => x.Details).FirstOrDefaultAsync(x => x.PurchaseOrderId == id);
        if (po == null) return NotFound();
        vm.PurchaseOrderId = id;

        Normalize(vm);
        var oldQty = po.Details.ToDictionary(d => d.VariantId, d => d.Quantity);
        var variants = await LoadVariants(vm.Items.Select(i => i.VariantId!).Concat(oldQty.Keys));
        Validate(vm, variants, oldQty);

        // Tồn kho sau khi sửa không được âm (ví dụ đã bán bớt số hàng vừa nhập)
        if (ModelState.IsValid)
        {
            foreach (var kv in variants)
            {
                var oq = oldQty.TryGetValue(kv.Key, out var a) ? a : 0;
                var nq = vm.Items.Where(i => i.VariantId == kv.Key).Sum(i => i.Quantity);
                var finalStock = (kv.Value.StockQuantity ?? 0) - oq + nq;
                if (finalStock < 0)
                    ModelState.AddModelError("", $"{kv.Key}: sau khi sửa tồn kho sẽ âm ({finalStock}) vì hàng đã được bán ra.");
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadForm();
            if (vm.Items.Count == 0) vm.Items.Add(new PoItemVm());
            return View("Form", vm);
        }

        foreach (var d in po.Details)
        {
            if (variants.TryGetValue(d.VariantId, out var ov))
                ov.StockQuantity = (ov.StockQuantity ?? 0) - d.Quantity;
        }

        var newIds = vm.Items.Select(i => i.VariantId!).ToHashSet();
        foreach (var d in po.Details.Where(d => !newIds.Contains(d.VariantId)).ToList())
            _db.PurchaseOrderDetails.Remove(d);

        foreach (var it in vm.Items)
        {
            var v = variants[it.VariantId!];
            v.StockQuantity = (v.StockQuantity ?? 0) + it.Quantity;

            var existing = po.Details.FirstOrDefault(d => d.VariantId == it.VariantId);
            if (existing != null)
            {
                existing.Quantity = it.Quantity;
                existing.ImportPrice = it.ImportPrice;
            }
            else
            {
                po.Details.Add(new PurchaseOrderDetail
                {
                    VariantId = it.VariantId!,
                    Quantity = it.Quantity,
                    ImportPrice = it.ImportPrice
                });
            }
        }

        po.SupplierId = vm.SupplierId!;
        po.EmployeeId = Nz(vm.EmployeeId);
        po.ImportDate = vm.ImportDate;
        po.Notes = Nz(vm.Notes);
        po.TotalAmount = vm.Items.Sum(i => i.Quantity * i.ImportPrice);

        await _db.SaveChangesAsync();
        Success($"Đã cập nhật phiếu nhập {id}.");
        return RedirectToAction(nameof(Details), new { id });
    }

    // ------------------------------------------------------------ XÓA
    [HttpPost("Delete/{id}"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var po = await _db.PurchaseOrders.Include(x => x.Details).FirstOrDefaultAsync(x => x.PurchaseOrderId == id);
        if (po == null) return NotFound();

        var variants = await LoadVariants(po.Details.Select(d => d.VariantId));
        foreach (var d in po.Details)
        {
            if (variants.TryGetValue(d.VariantId, out var v) && (v.StockQuantity ?? 0) < d.Quantity)
            {
                Fail($"Không thể xóa: tồn kho của {d.VariantId} hiện thấp hơn số lượng đã nhập ({d.Quantity}).");
                return RedirectToAction(nameof(Index));
            }
        }

        foreach (var d in po.Details)
        {
            if (variants.TryGetValue(d.VariantId, out var v))
                v.StockQuantity = (v.StockQuantity ?? 0) - d.Quantity;
        }

        _db.PurchaseOrderDetails.RemoveRange(po.Details);
        _db.PurchaseOrders.Remove(po);
        await _db.SaveChangesAsync();

        Success($"Đã xóa phiếu nhập {id} và hoàn lại tồn kho.");
        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------ HELPER
    private static void Normalize(PurchaseOrderFormVm vm)
    {
        vm.Items = (vm.Items ?? new List<PoItemVm>())
            .Where(i => !string.IsNullOrWhiteSpace(i.VariantId))
            .GroupBy(i => i.VariantId!.Trim())
            .Select(g => new PoItemVm
            {
                VariantId = g.Key,
                Quantity = g.Sum(x => x.Quantity),
                ImportPrice = g.First().ImportPrice
            }).ToList();
    }

    private async Task<Dictionary<string, ProductVariant>> LoadVariants(IEnumerable<string> ids)
    {
        var list = ids.Distinct().ToList();
        return await _db.ProductVariants.Where(v => list.Contains(v.VariantId)).ToDictionaryAsync(v => v.VariantId);
    }

    private void Validate(PurchaseOrderFormVm vm, Dictionary<string, ProductVariant> variants, Dictionary<string, int> oldQty)
    {
        if (string.IsNullOrWhiteSpace(vm.SupplierId))
            ModelState.AddModelError("", "Vui lòng chọn nhà cung cấp.");
        if (vm.Items.Count == 0)
            ModelState.AddModelError("", "Phiếu nhập cần ít nhất 1 sản phẩm.");

        foreach (var it in vm.Items)
        {
            if (!variants.ContainsKey(it.VariantId!))
                ModelState.AddModelError("", $"Biến thể '{it.VariantId}' không tồn tại.");
            if (it.Quantity <= 0)
                ModelState.AddModelError("", $"Số lượng của {it.VariantId} phải lớn hơn 0.");
            if (it.ImportPrice < 0)
                ModelState.AddModelError("", $"Giá nhập của {it.VariantId} không được âm.");
        }
    }
}
