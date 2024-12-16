using System.ComponentModel.DataAnnotations;

namespace WebBlog.Models.Requests
{
    public class EditPostRequestModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
    }
}
