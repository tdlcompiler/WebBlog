using System.Text.Json;
using WebBlog.Models;

namespace WebBlog.Repository.UserRepository
{
    public class FileUserRepository : IUserRepository
    {
        private readonly string _userFilePath;
        private List<UserModel> _users;

        public FileUserRepository(string userFilePath)
        {
            _userFilePath = userFilePath;
            _users = LoadFromFile(_userFilePath);
        }

        public void AddUser(UserModel user)
        {
            _users.Add(user);
            SaveUsers();
        }

        public UserModel? GetUserByEmail(string email)
        {
            return _users.FirstOrDefault(u => u.Email == email);
        }

        public UserModel? GetUserByRefreshToken(string token)
        {
            return _users.FirstOrDefault(u => u.RefreshToken == token);
        }

        public void UpdateUser(UserModel user)
        {
            SaveUsers();
        }

        public bool UserExistsByEmail(string email)
        {
            return _users.Any(u => u.Email == email);
        }

        private void SaveUsers()
        {
            SaveToFile(_userFilePath, _users);
        }

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
