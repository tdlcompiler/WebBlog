using System.ComponentModel.DataAnnotations;

namespace WebBlog.Models.Requests
{
    public class RegisterRequestModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Reader";
    }
}
