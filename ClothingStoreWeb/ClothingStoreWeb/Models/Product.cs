using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Products")]
public class Product
{
    [Key]
    [Display(Name = "Mã sản phẩm")]
    [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string ProductId { get; set; } = "";

    [Display(Name = "Tên sản phẩm")]
    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
    [StringLength(150, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? ProductName { get; set; }

    [Display(Name = "Chất liệu")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? MaterialId { get; set; }

    [Display(Name = "Form dáng")]
    [StringLength(55, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? FitType { get; set; }

    [Display(Name = "Mã kiểu (Style code)")]
    [StringLength(55, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? StyleCode { get; set; }

    [Display(Name = "Khối lượng (kg)")]
    public double? Weight { get; set; }

    [Display(Name = "Mùa")]
    [StringLength(55, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Season { get; set; }

    [Display(Name = "Thương hiệu")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? BrandId { get; set; }

    [Display(Name = "Xuất xứ")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? OriginCountryId { get; set; }

    [Display(Name = "Danh mục")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? CategoryId { get; set; }

    [Display(Name = "Đối tượng")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? AudienceId { get; set; }

    [Display(Name = "Bảo hành (ngày)")]
    public double? WarrantyDays { get; set; }

    [Display(Name = "Giảm giá (%)")]
    public double? DiscountPercent { get; set; } = 0;

    [Display(Name = "Website")]
    [StringLength(155, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Website { get; set; }

    [Display(Name = "Hướng dẫn bảo quản")]
    [StringLength(255, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? CareInstructions { get; set; }

    [Display(Name = "Mô tả")]
    [StringLength(500, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Description { get; set; }

    [Display(Name = "Ảnh đại diện")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Thumbnail { get; set; }

    [Display(Name = "Giá thấp nhất")]
    [Column(TypeName = "money")]
    public decimal? MinPrice { get; set; }

    [Display(Name = "Giá cao nhất")]
    [Column(TypeName = "money")]
    public decimal? MaxPrice { get; set; }

    [ForeignKey("MaterialId")]
    [ValidateNever]
    public Material? Material { get; set; }

    [ForeignKey("BrandId")]
    [ValidateNever]
    public Brand? Brand { get; set; }

    [ForeignKey("OriginCountryId")]
    [ValidateNever]
    public Country? OriginCountry { get; set; }

    [ForeignKey("CategoryId")]
    [ValidateNever]
    public Category? Category { get; set; }

    [ForeignKey("AudienceId")]
    [ValidateNever]
    public TargetAudience? Audience { get; set; }

    [ValidateNever] public List<ProductImage> Images { get; set; } = new();
    [ValidateNever] public List<ProductVariant> Variants { get; set; } = new();
    [ValidateNever] public List<Wishlist> Wishlists { get; set; } = new();
}