using System.ComponentModel.DataAnnotations;

namespace WebBlog.Models.Requests
{
    public class RefreshTokenRequestModel
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
