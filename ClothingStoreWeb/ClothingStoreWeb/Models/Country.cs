using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Countries")]
public class Country
{
    [Key]
    [Display(Name = "Mã quốc gia")]
    [Required(ErrorMessage = "Mã quốc gia là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    [RegularExpression(@"^[A-Za-z0-9_.@\-]+$", ErrorMessage = "{0} chỉ gồm chữ, số và các ký tự . _ - @")]
    public string CountryId { get; set; } = "";

    [Display(Name = "Tên quốc gia")]
    [Required(ErrorMessage = "Tên quốc gia là bắt buộc")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? CountryName { get; set; }

}
