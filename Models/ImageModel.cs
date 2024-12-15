using System.ComponentModel.DataAnnotations;

namespace WebBlog.Models
{
    public class ImageModel
    {
        [Required]
        public Guid ImageId { get; set; } = Guid.NewGuid();
        [Required]
        public Guid PostId { get; set; }
        [Required]
        public string? ImageUrl { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
