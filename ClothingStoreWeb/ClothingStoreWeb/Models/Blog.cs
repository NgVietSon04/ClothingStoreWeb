using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ClothingStoreWeb.Models;

[Table("Blogs")]
public class Blog
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Display(Name = "Mã bài viết")]
    public int BlogId { get; set; }

    [Display(Name = "Tiêu đề")]
    [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    [StringLength(250, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Title { get; set; }

    [Display(Name = "Nội dung")]
    public string? Description { get; set; }

    [Display(Name = "Ảnh đại diện")]
    [StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự")]
    public string? Thumbnail { get; set; }

    [Display(Name = "Ngày đăng")]
    public DateTime? CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "Số bình luận")]
    public int? CommentCount { get; set; } = 0;

}
