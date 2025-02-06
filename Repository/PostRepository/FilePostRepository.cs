using System.Text.Json;
using WebBlog.Models;
using WebBlog.Repository.UserRepository;

namespace WebBlog.Repository.PostRepository
{
    public class FilePostRepository : IPostRepository
    {
        private readonly string _postsFilePath;
        private List<PostModel> _posts;

        public FilePostRepository(string postsFilePath)
        {
            _postsFilePath = postsFilePath;
            _posts = LoadPostsFromFile(postsFilePath);
        }

        public void AddPost(PostModel post)
        {
            _posts.Add(post);
            SavePosts();
        }

        public PostModel? GetPostById(Guid postId)
        {
            return _posts.FirstOrDefault(p => p.PostId == postId);
        }

        public IEnumerable<PostModel> GetPosts(Func<PostModel, bool> predicate)
        {
            return _posts.Where(predicate);
        }

        public bool PostExists(Func<PostModel, bool> predicate)
        {
            return _posts.Any(predicate);
        }

        public void UpdatePost(PostModel post, ImageModel? image = null)
        {
            SavePosts();
        }

        private List<PostModel> LoadPostsFromFile(string postsFilePath)
        {
            if (!File.Exists(postsFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(postsFilePath) ?? throw new Exception("Invalid posts storage path."));
                File.WriteAllText(postsFilePath, "[]");
            }

            var json = File.ReadAllText(postsFilePath);
            return JsonSerializer.Deserialize<List<PostModel>>(json) ?? new List<PostModel>();
        }

        private void SavePosts()
        {
            var json = JsonSerializer.Serialize(_posts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_postsFilePath, json);
        }
    }
}
