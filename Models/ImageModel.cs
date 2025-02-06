using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBlog.Models
{
    public class ImageModel
    {
        [Required]
        [Key] public Guid ImageId { get; set; } = Guid.NewGuid();
        [Required]
        [ForeignKey("Post")] public Guid PostId { get; set; }
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
