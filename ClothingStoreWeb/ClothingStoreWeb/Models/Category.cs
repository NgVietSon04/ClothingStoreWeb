using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Categories")]
public class Category
{
    [Key]
    [Display(Name = "Mã danh mục")]
    [Required(ErrorMessage = "Mã danh mục là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    [RegularExpression(@"^[A-Za-z0-9_.@\-]+$", ErrorMessage = "{0} chỉ gồm chữ, số và các ký tự . _ - @")]
    public string CategoryId { get; set; } = "";

    [Display(Name = "Tên danh mục")]
    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? CategoryName { get; set; }

    [Display(Name = "Ảnh đại diện")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Thumbnail { get; set; }

}
