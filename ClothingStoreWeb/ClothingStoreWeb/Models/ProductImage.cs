using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("ProductImages")]
public class ProductImage
{
    [Display(Name = "Sản phẩm")]
    [Required(ErrorMessage = "Sản phẩm là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string ProductId { get; set; } = "";

    [Display(Name = "Tên file ảnh")]
    [Required(ErrorMessage = "Tên file ảnh là bắt buộc")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string ImageFileName { get; set; } = "";

    [Display(Name = "Thứ tự")]
    public short? SortOrder { get; set; }

    [ValidateNever] public Product? Product { get; set; }
}
