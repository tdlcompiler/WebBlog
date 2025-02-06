using System.ComponentModel.DataAnnotations;

namespace WebBlog.Models
{
    public class UserModel
    {
        [Key] public Guid UserId { get; set; } = Guid.NewGuid();

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Reader";
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
