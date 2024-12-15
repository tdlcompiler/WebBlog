namespace WebBlog.Models.Requests
{
    public class CreatePostRequestModel
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string IdempotencyKey { get; set; } = string.Empty;
    }
}
