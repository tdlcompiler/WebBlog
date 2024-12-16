using System.ComponentModel.DataAnnotations;

namespace WebBlog.Models.Requests
{
    public class CreatePostRequestModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
        [Required]
        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
