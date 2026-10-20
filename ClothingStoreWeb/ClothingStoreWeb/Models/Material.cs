using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Materials")]
public class Material
{
    [Key]
    [Display(Name = "Mã chất liệu")]
    [Required(ErrorMessage = "Mã chất liệu là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    [RegularExpression(@"^[A-Za-z0-9_.@\-]+$", ErrorMessage = "{0} chỉ gồm chữ, số và các ký tự . _ - @")]
    public string MaterialId { get; set; } = "";

    [Display(Name = "Tên chất liệu")]
    [Required(ErrorMessage = "Tên chất liệu là bắt buộc")]
    [StringLength(150, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? MaterialName { get; set; }

}
