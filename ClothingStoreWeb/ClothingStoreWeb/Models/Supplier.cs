using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Suppliers")]
public class Supplier
{
    [Key]
    [Display(Name = "Mã NCC")]
    [Required(ErrorMessage = "Mã NCC là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string SupplierId { get; set; } = "";

    [Display(Name = "Tên nhà cung cấp")]
    [Required(ErrorMessage = "Tên nhà cung cấp là bắt buộc")]
    [StringLength(150, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string SupplierName { get; set; } = "";

    [Display(Name = "Người liên hệ")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? ContactName { get; set; }

    [Display(Name = "Số điện thoại")]
    [StringLength(15, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Email")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Email { get; set; }

    [Display(Name = "Địa chỉ")]
    [StringLength(200, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Address { get; set; }

}
