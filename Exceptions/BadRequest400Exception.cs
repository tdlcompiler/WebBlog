namespace WebBlog.Exceptions
{
    public class BadRequest400Exception : Exception
    {
        public BadRequest400Exception(string message) : base(message) { }
    }
}
