using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebBlog.Exceptions;
using WebBlog.Services;

namespace WebBlog.Controllers
{
    /// <summary>
    /// Контроллер изображений (файлов на сервере).
    /// </summary>
    [ApiController]
    [Route("api/images")]
    public class ImageController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly ImageService _imageService;

        public ImageController(ImageService imageService, PostService postService)
        {
            _postService = postService;
            _imageService = imageService;
        }

        /// <summary>
        /// Получение файла с сервера.
        /// </summary>
        /// <param name="fileName">Относительный путь файла на сервере (имя.расширение).</param>
        /// <returns>Файл.</returns>
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [HttpGet("{fileName}")]
        [Authorize]
        public IActionResult GetFile(string fileName)
        {
            try
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

                if (!_postService.UserHasAccessToFile(userId, fileName))
                    return StatusCode(StatusCodes.Status403Forbidden, new { Message = "Access denied" });

                var image = _imageService.GetFile(fileName);
                return image;
            }
            catch (NotFound404Exception ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }
    }
}
