using WebBlog.Models;

namespace WebBlog.Repository.UserRepository
{
    public interface IUserRepository
    {
        public void AddUser(UserModel user);
        public void UpdateUser(UserModel user);
        public UserModel? GetUserByEmail(string email);
        public UserModel? GetUserByRefreshToken(string token);
        public bool UserExistsByEmail(string email);
    }
}
