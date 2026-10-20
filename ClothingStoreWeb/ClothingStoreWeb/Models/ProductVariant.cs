using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("ProductVariants")]
public class ProductVariant
{
    [Key]
    [Display(Name = "Mã biến thể")]
    [Required(ErrorMessage = "Mã biến thể là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string VariantId { get; set; } = "";

    [Display(Name = "Sản phẩm")]
    [Required(ErrorMessage = "Sản phẩm là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? ProductId { get; set; }

    [Display(Name = "Size")]
    [Required(ErrorMessage = "Size là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? SizeId { get; set; }

    [Display(Name = "Màu sắc")]
    [Required(ErrorMessage = "Màu sắc là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? ColorId { get; set; }

    [Display(Name = "Đơn giá (₫)")]
    [Required(ErrorMessage = "Đơn giá (₫) là bắt buộc")]
    public double? UnitPrice { get; set; }

    [Display(Name = "Giảm giá (%)")]
    public double? Discount { get; set; } = 0;

    [Display(Name = "Tồn kho")]
    [Required(ErrorMessage = "Tồn kho là bắt buộc")]
    public int? StockQuantity { get; set; } = 0;

    [Display(Name = "Ảnh biến thể")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Thumbnail { get; set; }

    [Display(Name = "Video (URL)")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? VideoUrl { get; set; }

    [ValidateNever] public Product? Product { get; set; }
    [ValidateNever] public Size? Size { get; set; }
    [ValidateNever] public Color? Color { get; set; }
    [ValidateNever] public List<ProductVariantImage> Images { get; set; } = new();
}
