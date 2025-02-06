using Microsoft.EntityFrameworkCore;
using WebBlog.Models;

namespace WebBlog.Repository.UserRepository
{
    public class PostgresPostRepository : IPostRepository
    {
        private readonly BlogDbContext _context;

        public PostgresPostRepository(BlogDbContext context)
        {
            _context = context;
        }

        public void AddPost(PostModel post)
        {
            _context.Posts.Add(post);
            _context.SaveChanges();
        }

        public void UpdatePost(PostModel post, ImageModel? image = null)
        {
            _context.Posts.Update(post);
            if (image != null) _context.Images.Add(image);
            _context.SaveChanges();
        }

        public PostModel? GetPostById(Guid postId)
        {
            return _context.Posts
                .Include(post => post.Images)
                .FirstOrDefault(post => post.PostId == postId);
        }

        public IEnumerable<PostModel> GetPosts(Func<PostModel, bool> predicate)
        {
            return _context.Posts.Include(post => post.Images).Where(predicate).ToList();
        }

        public bool PostExists(Func<PostModel, bool> predicate)
        {
            return _context.Posts.Include(post => post.Images).Any(predicate);
        }
    }
}