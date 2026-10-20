using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Carts")]
public class Cart
{
    [Key]
    [Display(Name = "Mã giỏ")]
    [Required(ErrorMessage = "Mã giỏ là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    [RegularExpression(@"^[A-Za-z0-9_.@\-]+$", ErrorMessage = "{0} chỉ gồm chữ, số và các ký tự . _ - @")]
    public string CartId { get; set; } = "";

    [Display(Name = "Tài khoản")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Username { get; set; }

    [Display(Name = "Ngày tạo")]
    public DateTime? CreatedDate { get; set; }

    [Display(Name = "Cập nhật")]
    public DateTime? UpdatedDate { get; set; }

    [Display(Name = "Ghi chú")]
    [StringLength(250, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Notes { get; set; }

    [ValidateNever]
    public User? User { get; set; }

    // Đã đổi từ Details thành CartDetails
    [ValidateNever] public List<CartDetail> CartDetails { get; set; } = new();
}