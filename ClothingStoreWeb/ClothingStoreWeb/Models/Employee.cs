using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Employees")]
public class Employee
{
    [Key]
    [Display(Name = "Mã NV")]
    [Required(ErrorMessage = "Mã NV là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string EmployeeId { get; set; } = "";

    [Display(Name = "Ảnh đại diện")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Avatar { get; set; }

    [Display(Name = "Họ và tên")]
    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? FullName { get; set; }

    [Display(Name = "Tài khoản đăng nhập")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Username { get; set; }

    [Display(Name = "Ngày sinh")]
    [Column(TypeName = "date")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Điện thoại 1")]
    [StringLength(15, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? PhoneNumber1 { get; set; }

    [Display(Name = "Điện thoại 2")]
    [StringLength(15, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? PhoneNumber2 { get; set; }

    [Display(Name = "Địa chỉ")]
    [StringLength(150, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Address { get; set; }

    [Display(Name = "Chức vụ")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Position { get; set; }

    [Display(Name = "Ghi chú")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Notes { get; set; }

    [ForeignKey("Username")]
    [ValidateNever]
    public User? Account { get; set; }
}
