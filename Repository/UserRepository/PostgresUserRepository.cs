using WebBlog.Models;

namespace WebBlog.Repository.UserRepository
{
    public class PostgresUserRepository : IUserRepository
    {
        private readonly BlogDbContext _context;

        public PostgresUserRepository(BlogDbContext context)
        {
            _context = context;
        }

        public void AddUser(UserModel user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void UpdateUser(UserModel user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public UserModel? GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public UserModel? GetUserByRefreshToken(string token)
        {
            return _context.Users.FirstOrDefault(u => u.RefreshToken == token);
        }

        public bool UserExistsByEmail(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }
    }
}
