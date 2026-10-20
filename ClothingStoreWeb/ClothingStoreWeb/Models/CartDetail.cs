using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("CartDetails")]
public class CartDetail
{
    [Display(Name = "Mã giỏ")]
    [Required(ErrorMessage = "Mã giỏ là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string CartId { get; set; } = "";

    [Display(Name = "Biến thể")]
    [Required(ErrorMessage = "Biến thể là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string VariantId { get; set; } = "";

    [Display(Name = "Số lượng")]
    public int Quantity { get; set; }

    [Display(Name = "Ngày thêm")]
    public DateTime? AddedDate { get; set; }

    [ValidateNever] public Cart? Cart { get; set; }
    [ValidateNever] public ProductVariant? Variant { get; set; }
}
