using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("OrderDetails")]
public class OrderDetail
{
    [Display(Name = "Mã đơn")]
    [Required(ErrorMessage = "Mã đơn là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string OrderId { get; set; } = "";

    [Display(Name = "Biến thể")]
    [Required(ErrorMessage = "Biến thể là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string VariantId { get; set; } = "";

    [Display(Name = "Số lượng")]
    public int? Quantity { get; set; }

    [Display(Name = "Đơn giá")]
    [Column(TypeName = "money")]
    public decimal? UnitPrice { get; set; }

    [Display(Name = "Giảm giá (%)")]
    public double? Discount { get; set; }

    [Display(Name = "Ghi chú")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Notes { get; set; }

    [ValidateNever] public Order? Order { get; set; }
    [ValidateNever] public ProductVariant? Variant { get; set; }
}
