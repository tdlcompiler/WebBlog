namespace WebBlog.Services
{
    using Microsoft.Extensions.Configuration;
    using WebBlog.Exceptions;
    using WebBlog.Models;
    using WebBlog.Repository.PostRepository;
    using WebBlog.Repository.UserRepository;

    public class PostService
    {
        private IPostRepository _postRepository;

        public PostService(string postFilePath)
        {
            _postRepository = new FilePostRepository(postFilePath);
        }

        public PostService(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public PostModel CreatePost(Guid authorId, string title, string content, string idempotencyKey)
        {
            if (_postRepository.PostExists(p => p.IdempotencyKey == idempotencyKey)) throw new Conflict409Exception("Idempotency key already used.");

            var post = new PostModel
            {
                AuthorId = authorId,
                Title = title,
                Content = content,
                IdempotencyKey = idempotencyKey
            };

            _postRepository.AddPost(post);

            return post;
        }

        public void AddImageToPost(Guid authorId, Guid postId, string imageUrl)
        {
            var post = _postRepository.GetPostById(postId) ?? throw new NotFound404Exception("Post not found.");
            if (post.AuthorId != authorId) throw new Forbidden403Exception("Access denied.");
            ImageModel image = new ImageModel
            {
                PostId = postId,
                ImageUrl = imageUrl
            };
            post.Images.Add(image);

            _postRepository.UpdatePost(post, image);
        }

        public void EditPost(Guid userId, Guid postId, string title, string content)
        {
            var post = _postRepository.GetPostById(postId);
            if (post == null) throw new NotFound404Exception("Post not found.");
            if (post.AuthorId != userId) throw new Forbidden403Exception("Access denied.");

            post.Title = title;
            post.Content = content;
            post.UpdatedAt = DateTime.UtcNow;

            _postRepository.UpdatePost(post);
        }

        public void PublishPost(Guid userId, Guid postId)
        {
            var post = _postRepository.GetPostById(postId);
            if (post == null) throw new NotFound404Exception("Post not found.");
            if (post.AuthorId != userId) throw new Forbidden403Exception("Access denied.");
            if (post.Status == "Published") throw new Conflict409Exception("Post already published.");

            post.Status = "Published";

            _postRepository.UpdatePost(post);
        }

        public void DeleteImageFromPost(Guid userId, Guid postId, Guid imageId, string imageStoragePath)
        {
            var post = _postRepository.GetPostById(postId);
            if (post == null) throw new NotFound404Exception("Post not found.");
            if (post.AuthorId != userId) throw new Forbidden403Exception("Access denied.");

            var image = post.Images.FirstOrDefault(i => i.ImageId == imageId);
            if (image == null) throw new NotFound404Exception("Image not found.");

            DeleteImageFromFileSystem(Path.Combine(imageStoragePath, image.ImageUrl));
            post.Images.Remove(image);

            _postRepository.UpdatePost(post);
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
            return _postRepository.GetPosts(p => p.AuthorId == userId);
        }

        public IEnumerable<PostModel> GetPublishedPosts()
        {
            return _postRepository.GetPosts(p => p.Status == "Published");
        }

        public bool UserHasAccessToFile(Guid userId, string fileName)
        {
            return _postRepository.PostExists(p => ((p.AuthorId == userId) || (p.Status == "Published")) && p.Images.Any(i => i.ImageUrl.Contains(fileName)));
        }
    }
}
