namespace WebBlog.Models.Requests
{
    public class EditPostRequestModel
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
