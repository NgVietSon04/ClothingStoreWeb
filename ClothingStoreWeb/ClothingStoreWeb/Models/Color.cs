using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Colors")]
public class Color
{
    [Key]
    [Display(Name = "Mã màu")]
    [Required(ErrorMessage = "Mã màu là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    [RegularExpression(@"^[A-Za-z0-9_.@\-]+$", ErrorMessage = "{0} chỉ gồm chữ, số và các ký tự . _ - @")]
    public string ColorId { get; set; } = "";

    [Display(Name = "Tên màu")]
    [Required(ErrorMessage = "Tên màu là bắt buộc")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? ColorName { get; set; }

}
