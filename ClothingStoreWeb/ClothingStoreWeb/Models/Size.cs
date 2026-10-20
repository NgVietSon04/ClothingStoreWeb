using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Sizes")]
public class Size
{
    [Key]
    [Display(Name = "Mã size")]
    [Required(ErrorMessage = "Mã size là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    [RegularExpression(@"^[A-Za-z0-9_.@\-]+$", ErrorMessage = "{0} chỉ gồm chữ, số và các ký tự . _ - @")]
    public string SizeId { get; set; } = "";

    [Display(Name = "Mô tả size")]
    [Required(ErrorMessage = "Mô tả size là bắt buộc")]
    [StringLength(150, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? SizeName { get; set; }

}
