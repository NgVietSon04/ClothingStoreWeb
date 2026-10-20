using Microsoft.EntityFrameworkCore;
using ClothingStoreWeb.Models;

namespace ClothingStoreWeb.Services;

public interface IDashboardService
{
    Task<DashboardVm> GetAsync();
}

public class DashboardService : IDashboardService
{
    private const int LowStockThreshold = 10;
    private readonly ClothingStoreDbContext _db;

    public DashboardService(ClothingStoreDbContext db) => _db = db;

    public async Task<DashboardVm> GetAsync()
    {
        var vm = new DashboardVm
        {
            Revenue = await _db.Orders.SumAsync(o => o.TotalAmount) ?? 0m,
            OrderCount = await _db.Orders.CountAsync(),
            ProductCount = await _db.Products.CountAsync(),
            CustomerCount = await _db.Customers.CountAsync(),
            LowStockCount = await _db.ProductVariants.CountAsync(v => v.StockQuantity <= LowStockThreshold)
        };

        // Doanh thu 6 tháng gần nhất
        var now = DateTime.Now;
        var start = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var rows = await _db.Orders.AsNoTracking()
            .Where(o => o.OrderDate >= start)
            .Select(o => new { o.OrderDate, o.TotalAmount })
            .ToListAsync();

        for (var i = 0; i < 6; i++)
        {
            var month = start.AddMonths(i);
            var total = rows
                .Where(r => r.OrderDate.HasValue && r.OrderDate.Value.Year == month.Year && r.OrderDate.Value.Month == month.Month)
                .Sum(r => r.TotalAmount ?? 0m);
            vm.MonthlyRevenue.Add(new MonthRevenueVm { Label = month.ToString("MM/yyyy"), Total = total });
        }
        vm.RevenueThisMonth = vm.MonthlyRevenue.Last().Total;

        // Top 5 biến thể bán chạy
        var top = await _db.OrderDetails.AsNoTracking()
            .GroupBy(d => d.VariantId)
            .Select(g => new { VariantId = g.Key, Qty = g.Sum(x => x.Quantity ?? 0) })
            .OrderByDescending(x => x.Qty)
            .Take(5)
            .ToListAsync();

        var ids = top.Select(t => t.VariantId).ToList();
        var names = await _db.ProductVariants.AsNoTracking()
            .Where(v => ids.Contains(v.VariantId))
            .Select(v => new { v.VariantId, Name = v.Product != null ? v.Product.ProductName : null })
            .ToListAsync();

        vm.TopProducts = top.Select(t => new TopProductVm
        {
            VariantId = t.VariantId,
            Name = names.FirstOrDefault(n => n.VariantId == t.VariantId)?.Name ?? t.VariantId,
            Quantity = t.Qty
        }).ToList();

        vm.RecentOrders = await _db.Orders.AsNoTracking()
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Take(8)
            .ToListAsync();

        vm.LowStock = await _db.ProductVariants.AsNoTracking()
            .Include(v => v.Product)
            .Where(v => v.StockQuantity <= LowStockThreshold)
            .OrderBy(v => v.StockQuantity)
            .Take(8)
            .ToListAsync();

        return vm;
    }
}
