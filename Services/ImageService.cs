using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using WebBlog.Exceptions;

namespace WebBlog.Services
{
    public class ImageService
    {
        private readonly string _uploadsFolder;

        public ImageService(string imagesPath)
        {
            _uploadsFolder = imagesPath;
        }

        public FileResult GetFile(string fileName)
        {
            var filePath = Path.Combine(_uploadsFolder, fileName);
            if (!File.Exists(filePath)) throw new NotFound404Exception("File not found.");

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var contentType = GetContentType(filePath);

            return new FileStreamResult(fileStream, contentType)
            {
                FileDownloadName = fileName
            };
        }

        private string GetContentType(string path)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType))
            {
                contentType = "application/octet-stream";
            }
            return contentType;
        }
    }
}
