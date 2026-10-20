using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("CustomerAddresses")]
public class CustomerAddress
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Mã")]
    public int AddressId { get; set; }

    [Display(Name = "Khách hàng")]
    [Required(ErrorMessage = "Khách hàng là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string CustomerId { get; set; } = "";

    [Display(Name = "Người nhận")]
    [Required(ErrorMessage = "Người nhận là bắt buộc")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string RecipientName { get; set; } = "";

    [Display(Name = "Số điện thoại")]
    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [StringLength(15, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string PhoneNumber { get; set; } = "";

    [Display(Name = "Địa chỉ chi tiết")]
    [Required(ErrorMessage = "Địa chỉ chi tiết là bắt buộc")]
    [StringLength(255, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string AddressDetail { get; set; } = "";

    [Display(Name = "Địa chỉ mặc định")]
    public bool IsDefault { get; set; }

    [ValidateNever] public Customer? Customer { get; set; }
}
