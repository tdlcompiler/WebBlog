using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace WebBlog.Models
{
    public class UserModel
    {
        public Guid UserId { get; set; } = Guid.NewGuid();

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Reader";
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime RefreshTokenExpiryTime { get; set; }

        public static List<UserModel> LoadFromFile(string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath ?? throw new Exception("Users storage creation error: invalid directoryPath."));
            }

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "[]");
                return new List<UserModel>();
            }

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<UserModel>>(json) ?? new List<UserModel>();
        }

        public static void SaveToFile(string filePath, List<UserModel> users)
        {
            var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
    }
}
