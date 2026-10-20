using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Orders")]
public class Order
{
    [Key]
    [Display(Name = "Mã đơn")]
    [Required(ErrorMessage = "Mã đơn là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string OrderId { get; set; } = "";

    [Display(Name = "Ngày đặt")]
    [DataType(DataType.DateTime)]
    public DateTime? OrderDate { get; set; }

    [Display(Name = "Khách hàng")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? CustomerId { get; set; }

    [Display(Name = "Nhân viên")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? EmployeeId { get; set; }

    [Display(Name = "Tổng tiền")]
    [Column(TypeName = "money")]
    public decimal? TotalAmount { get; set; }

    [Display(Name = "Giảm giá đơn (%)")]
    public double? OrderDiscount { get; set; }

    [Display(Name = "Thanh toán")]
    public byte? PaymentMethod { get; set; }

    [Display(Name = "Mã số thuế")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? TaxCode { get; set; }

    [Display(Name = "Thông tin thuế")]
    [StringLength(250, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? TaxInfo { get; set; }

    [Display(Name = "Ghi chú")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Notes { get; set; }

    [ValidateNever] public Customer? Customer { get; set; }
    [ValidateNever] public Employee? Employee { get; set; }

    [ValidateNever] public List<OrderDetail> OrderDetails { get; set; } = new();
}