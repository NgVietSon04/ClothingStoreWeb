using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("PurchaseOrders")]
public class PurchaseOrder
{
    [Key]
    [Display(Name = "Mã phiếu nhập")]
    [Required(ErrorMessage = "Mã phiếu nhập là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string PurchaseOrderId { get; set; } = "";

    [Display(Name = "Nhà cung cấp")]
    [Required(ErrorMessage = "Nhà cung cấp là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string SupplierId { get; set; } = "";

    [Display(Name = "Nhân viên")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? EmployeeId { get; set; }

    [Display(Name = "Ngày nhập")]
    [DataType(DataType.DateTime)]
    public DateTime? ImportDate { get; set; }

    [Display(Name = "Tổng tiền")]
    [Column(TypeName = "money")]
    public decimal? TotalAmount { get; set; }

    [Display(Name = "Ghi chú")]
    [StringLength(250, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Notes { get; set; }

    [ValidateNever] public Supplier? Supplier { get; set; }
    [ValidateNever] public Employee? Employee { get; set; }
    [ValidateNever] public List<PurchaseOrderDetail> Details { get; set; } = new();
}
