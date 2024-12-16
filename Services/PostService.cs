namespace WebBlog.Services
{
    using System.Text.Json;
    using WebBlog.Exceptions;
    using WebBlog.Models;

    public class PostService
    {
        private readonly string _postFilePath;
        private List<PostModel> _posts;

        public PostService(string postFilePath)
        {
            _postFilePath = postFilePath;
            _posts = LoadPostsFromFile();
        }

        public PostModel CreatePost(Guid authorId, string title, string content, string idempotencyKey)
        {
            if (_posts.Any(p => p.IdempotencyKey == idempotencyKey)) throw new Conflict409Exception("Idempotency key already used.");

            var post = new PostModel
            {
                AuthorId = authorId,
                Title = title,
                Content = content,
                IdempotencyKey = idempotencyKey
            };

            _posts.Add(post);
            SavePosts();

            return post;
        }

        public void AddImageToPost(Guid authorId, Guid postId, string imageUrl)
        {
            var post = _posts.FirstOrDefault(p => p.PostId == postId) ?? throw new NotFound404Exception("Post not found.");
            if (post.AuthorId != authorId) throw new Forbidden403Exception("Access denied.");

            post.Images.Add(new ImageModel
            {
                PostId = postId,
                ImageUrl = imageUrl
            });

            SavePosts();
        }

        public void EditPost(Guid userId, Guid postId, string title, string content)
        {
            var post = _posts.FirstOrDefault(p => p.PostId == postId);
            if (post == null) throw new NotFound404Exception("Post not found.");
            if (post.AuthorId != userId) throw new Forbidden403Exception("Access denied.");

            post.Title = title;
            post.Content = content;
            post.UpdatedAt = DateTime.UtcNow;

            SavePosts();
        }

        public void PublishPost(Guid userId, Guid postId)
        {
            var post = _posts.FirstOrDefault(p => p.PostId == postId);
            if (post == null) throw new NotFound404Exception("Post not found.");
            if (post.AuthorId != userId) throw new Forbidden403Exception("Access denied.");
            if (post.Status == "Published") throw new Conflict409Exception("Post already published.");

            post.Status = "Published";

            SavePosts();
        }

        public void DeleteImageFromPost(Guid userId, Guid postId, Guid imageId, string imageStoragePath)
        {
            var post = _posts.FirstOrDefault(p => p.PostId == postId);
            if (post == null) throw new NotFound404Exception("Post not found.");
            if (post.AuthorId != userId) throw new Forbidden403Exception("Access denied.");

            var image = post.Images.FirstOrDefault(i => i.ImageId == imageId);
            if (image == null) throw new NotFound404Exception("Image not found.");

            DeleteImageFromFileSystem(Path.Combine(imageStoragePath, image.ImageUrl));
            post.Images.Remove(image);
            SavePosts();
        }

        private void DeleteImageFromFileSystem(string imageFullPath)
        {
            if (File.Exists(imageFullPath))
            {
                File.Delete(imageFullPath);
            }
            else
            {
                throw new FileNotFoundException("Image not found in FS", imageFullPath);
            }
        }

        public string SaveImageToFileSystem(IFormFile image, string imageStoragePath)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(imageStoragePath, fileName);

            Directory.CreateDirectory(imageStoragePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(stream);
            }

            return fileName;
        }

        public IEnumerable<PostModel> GetAuthorPosts(Guid userId)
        {
            return _posts
                .Where(p => p.AuthorId == userId);
        }

        public IEnumerable<PostModel> GetPublishedPosts()
        {
            return _posts.Where(p => p.Status == "Published");
        }

        private List<PostModel> LoadPostsFromFile()
        {
            if (!File.Exists(_postFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_postFilePath) ?? throw new Exception("Invalid posts storage path."));
                File.WriteAllText(_postFilePath, "[]");
            }

            var json = File.ReadAllText(_postFilePath);
            return JsonSerializer.Deserialize<List<PostModel>>(json) ?? new List<PostModel>();
        }

        private void SavePosts()
        {
            var json = JsonSerializer.Serialize(_posts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_postFilePath, json);
        }

        public bool UserHasAccessToFile(Guid userId, string fileName)
        {
            return _posts.Any(p => p.AuthorId == userId && p.Images.Any(i => i.ImageUrl.Contains(fileName)));
        }
    }
}
