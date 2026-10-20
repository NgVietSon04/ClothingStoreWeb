using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Promotions")]
public class Promotion
{
    [Key]
    [Display(Name = "Mã KM")]
    [Required(ErrorMessage = "Mã KM là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string PromotionId { get; set; } = "";

    [Display(Name = "Mã giảm giá (Code)")]
    [Required(ErrorMessage = "Mã giảm giá (Code) là bắt buộc")]
    [StringLength(50, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string Code { get; set; } = "";

    [Display(Name = "Mô tả")]
    [StringLength(250, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Description { get; set; }

    [Display(Name = "Loại giảm")]
    [Required(ErrorMessage = "Loại giảm là bắt buộc")]
    public byte DiscountType { get; set; }

    [Display(Name = "Giá trị giảm")]
    [Required(ErrorMessage = "Giá trị giảm là bắt buộc")]
    public double DiscountValue { get; set; }

    [Display(Name = "Đơn tối thiểu (₫)")]
    [Column(TypeName = "money")]
    public decimal? MinOrderValue { get; set; }

    [Display(Name = "Giảm tối đa (₫)")]
    [Column(TypeName = "money")]
    public decimal? MaxDiscountAmount { get; set; }

    [Display(Name = "Bắt đầu")]
    [Required(ErrorMessage = "Bắt đầu là bắt buộc")]
    [DataType(DataType.DateTime)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Display(Name = "Kết thúc")]
    [Required(ErrorMessage = "Kết thúc là bắt buộc")]
    [DataType(DataType.DateTime)]
    public DateTime EndDate { get; set; } = DateTime.Today.AddMonths(1);

    [Display(Name = "Giới hạn lượt dùng")]
    public int? UsageLimit { get; set; }

    [Display(Name = "Đã dùng")]
    public int? UsedCount { get; set; }

}
