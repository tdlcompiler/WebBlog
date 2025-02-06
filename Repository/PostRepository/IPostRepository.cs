using WebBlog.Models;

namespace WebBlog.Repository.UserRepository
{
    public interface IPostRepository
    {
        public void AddPost(PostModel post);
        public void UpdatePost(PostModel post, ImageModel? image = null);
        public PostModel? GetPostById(Guid postId);
        public IEnumerable<PostModel> GetPosts(Func<PostModel, bool> predicate);
        public bool PostExists(Func<PostModel, bool> predicate);
    }
}
