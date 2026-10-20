using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Users")]
public class User
{
    [Key]
    [Display(Name = "Tên đăng nhập")]
    [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    [RegularExpression(@"^[A-Za-z0-9_.@\-]+$", ErrorMessage = "{0} chỉ gồm chữ, số và các ký tự . _ - @")]
    public string Username { get; set; } = "";

    [Display(Name = "Mật khẩu")]
    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(256, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string Password { get; set; } = "";

    [Display(Name = "Vai trò")]
    [Required(ErrorMessage = "Vai trò là bắt buộc")]
    public byte? UserRole { get; set; }

}
