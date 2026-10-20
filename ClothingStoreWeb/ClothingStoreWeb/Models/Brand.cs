using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Brands")]
public class Brand
{
    [Key]
    [Display(Name = "Mã thương hiệu")]
    [Required(ErrorMessage = "Mã thương hiệu là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    [RegularExpression(@"^[A-Za-z0-9_.@\-]+$", ErrorMessage = "{0} chỉ gồm chữ, số và các ký tự . _ - @")]
    public string BrandId { get; set; } = "";

    [Display(Name = "Tên thương hiệu")]
    [Required(ErrorMessage = "Tên thương hiệu là bắt buộc")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? BrandName { get; set; }

    [Display(Name = "Quốc gia")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? BrandCountryId { get; set; }

    [ValidateNever] public Country? BrandCountry { get; set; }
}
