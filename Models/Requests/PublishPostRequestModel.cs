using System.ComponentModel.DataAnnotations;

namespace WebBlog.Models.Requests
{
    public class PublishPostRequestModel
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }
}
