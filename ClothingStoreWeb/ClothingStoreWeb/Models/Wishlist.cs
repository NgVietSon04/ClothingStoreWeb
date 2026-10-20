using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Wishlists")]
public class Wishlist
{
    [Display(Name = "Khách hàng")]
    [Required(ErrorMessage = "Khách hàng là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string CustomerId { get; set; } = "";

    [Display(Name = "Sản phẩm")]
    [Required(ErrorMessage = "Sản phẩm là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string ProductId { get; set; } = "";

    [Display(Name = "Ngày thêm")]
    public DateTime? AddedDate { get; set; }

    [ValidateNever] public Customer? Customer { get; set; }
    [ValidateNever] public Product? Product { get; set; }
}
