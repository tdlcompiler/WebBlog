using System.ComponentModel.DataAnnotations;

namespace WebBlog.Models
{
    public class PostModel
    {
        [Required]
        public Guid PostId { get; set; } = Guid.NewGuid();
        [Required]
        public Guid AuthorId { get; set; }
        [Required]
        public string? IdempotencyKey { get; set; }
        [Required]
        public string? Title { get; set; }
        [Required]
        public string? Content { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public string Status { get; set; } = "Draft";
        public List<ImageModel> Images { get; set; } = [];
    }
}
