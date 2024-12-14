namespace WebBlog.Exceptions
{
    public class Forbidden403Exception : Exception
    {
        public Forbidden403Exception(string message) : base(message) { }
    }
}
