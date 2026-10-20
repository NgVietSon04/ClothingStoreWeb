using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("TargetAudiences")]
public class TargetAudience
{
    [Key]
    [Display(Name = "Mã đối tượng")]
    [Required(ErrorMessage = "Mã đối tượng là bắt buộc")]
    [StringLength(25, ErrorMessage = "{0} tối đa {1} ký tự")]
    [RegularExpression(@"^[A-Za-z0-9_.@\-]+$", ErrorMessage = "{0} chỉ gồm chữ, số và các ký tự . _ - @")]
    public string AudienceId { get; set; } = "";

    [Display(Name = "Tên đối tượng")]
    [Required(ErrorMessage = "Tên đối tượng là bắt buộc")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? AudienceName { get; set; }

}
