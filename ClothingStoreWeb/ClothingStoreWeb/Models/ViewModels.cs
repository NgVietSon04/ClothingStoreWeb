using System.ComponentModel.DataAnnotations;

namespace ClothingStoreWeb.Models;

// ---------------------------------------------------------------- Đăng nhập
public class LoginVm
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
    public string? Username { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
    public string? Password { get; set; }

    public bool RememberMe { get; set; }
}

// ---------------------------------------------------------------- Dashboard
public class DashboardVm
{
    public decimal Revenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int OrderCount { get; set; }
    public int ProductCount { get; set; }
    public int CustomerCount { get; set; }
    public int LowStockCount { get; set; }
    public List<MonthRevenueVm> MonthlyRevenue { get; set; } = new();
    public List<TopProductVm> TopProducts { get; set; } = new();
    public List<Order> RecentOrders { get; set; } = new();
    public List<ProductVariant> LowStock { get; set; } = new();
}

public class MonthRevenueVm
{
    public string Label { get; set; } = "";
    public decimal Total { get; set; }
}

public class TopProductVm
{
    public string VariantId { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
}

// ---------------------------------------------------------------- Đơn hàng
public class VariantPickVm
{
    public string Id { get; set; } = "";
    public string Text { get; set; } = "";
    public decimal Price { get; set; }
    public double Discount { get; set; }
    public decimal Cost { get; set; }
    public int Stock { get; set; }
}

public class OrderItemVm
{
    public string? VariantId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public double Discount { get; set; }
    public string? Notes { get; set; }
}

public class OrderFormVm
{
    public string? OrderId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.Now;
    public string? CustomerId { get; set; }
    public string? EmployeeId { get; set; }
    public byte PaymentMethod { get; set; }
    public double OrderDiscount { get; set; }
    public string? TaxCode { get; set; }
    public string? TaxInfo { get; set; }

    [StringLength(100, ErrorMessage = "Ghi chú tối đa 100 ký tự")]
    public string? Notes { get; set; }

    public List<OrderItemVm> Items { get; set; } = new();
}

// ---------------------------------------------------------------- Nhập kho
public class PoItemVm
{
    public string? VariantId { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal ImportPrice { get; set; }
}

public class PurchaseOrderFormVm
{
    public string? PurchaseOrderId { get; set; }
    public DateTime ImportDate { get; set; } = DateTime.Now;
    public string? SupplierId { get; set; }
    public string? EmployeeId { get; set; }

    [StringLength(250, ErrorMessage = "Ghi chú tối đa 250 ký tự")]
    public string? Notes { get; set; }

    public List<PoItemVm> Items { get; set; } = new();
}
