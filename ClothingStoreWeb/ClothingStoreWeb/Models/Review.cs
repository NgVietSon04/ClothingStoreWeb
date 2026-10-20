using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Reviews")]
public class Review
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Mã")]
    public int ReviewId { get; set; }

    [Display(Name = "Sản phẩm")]
    [Required(ErrorMessage = "Sản phẩm là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string ProductId { get; set; } = "";

    [Display(Name = "Khách hàng")]
    [Required(ErrorMessage = "Khách hàng là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string CustomerId { get; set; } = "";

    [Display(Name = "Số sao")]
    [Required(ErrorMessage = "Số sao là bắt buộc")]
    public byte Rating { get; set; } = 5;

    [Display(Name = "Nhận xét")]
    public string? Comment { get; set; }

    [Display(Name = "Ngày đánh giá")]
    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    [ValidateNever] public Product? Product { get; set; }
    [ValidateNever] public Customer? Customer { get; set; }
}
