using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("PurchaseOrderDetails")]
public class PurchaseOrderDetail
{
    [Display(Name = "Mã phiếu nhập")]
    [Required(ErrorMessage = "Mã phiếu nhập là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string PurchaseOrderId { get; set; } = "";

    [Display(Name = "Biến thể")]
    [Required(ErrorMessage = "Biến thể là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string VariantId { get; set; } = "";

    [Display(Name = "Số lượng")]
    public int Quantity { get; set; }

    [Display(Name = "Giá nhập")]
    [Column(TypeName = "money")]
    public decimal ImportPrice { get; set; }

    [ValidateNever] public PurchaseOrder? PurchaseOrder { get; set; }
    [ValidateNever] public ProductVariant? Variant { get; set; }
}
