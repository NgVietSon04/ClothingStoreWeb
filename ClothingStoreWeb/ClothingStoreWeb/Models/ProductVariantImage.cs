using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("ProductVariantImages")]
public class ProductVariantImage
{
    [Display(Name = "Biến thể")]
    [Required(ErrorMessage = "Biến thể là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string VariantId { get; set; } = "";

    [Display(Name = "Tên file ảnh")]
    [Required(ErrorMessage = "Tên file ảnh là bắt buộc")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string ImageFileName { get; set; } = "";

    [Display(Name = "Thứ tự")]
    public short? SortOrder { get; set; }

    [ValidateNever] public ProductVariant? Variant { get; set; }
}
