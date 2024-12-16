namespace WebBlog.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using WebBlog.Exceptions;
    using WebBlog.Models;
    using WebBlog.Models.Requests;
    using WebBlog.Services;

    /// <summary>
    /// Контроллер постов.
    /// </summary>
    [ApiController]
    [Route("api/posts")]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly string _imageStoragePath = "data/images";

        public PostController(PostService postService)
        {
            _postService = postService;
        }

        /// <summary>
        /// Создание нового поста.
        /// </summary>
        /// <param name="dto">Модель данных для создания поста, включающая заголовок, контент (текст) и ключ идемпотентности.</param>
        /// <returns>Результат (статус-код).</returns>
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [Authorize(Roles = "Author")]
        public IActionResult CreatePost([FromBody] CreatePostRequestModel dto)
        {
            try
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
                var post = _postService.CreatePost(userId, dto.Title, dto.Content, dto.IdempotencyKey);
                return Created("", post);
            }
            catch (Conflict409Exception ex)
            {
                return Conflict(new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }

        /// <summary>
        /// Добавление изображения к посту.
        /// </summary>
        /// <param name="postId">Guid поста.</param>
        /// <param name="image">Файл с изображением, загруженный через форму.</param>
        /// <returns>Результат (статус-код) с сообщением.</returns>
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [HttpPost("{postId}/images")]
        [Authorize(Roles = "Author")]
        public IActionResult AddImageToPost(Guid postId, [FromForm] IFormFile image)
        {
            try
            {
                if (image == null) return BadRequest(new { Message = "No file uploaded." });

                var authorId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
                var imageUrl = _postService.SaveImageToFileSystem(image, _imageStoragePath);
                _postService.AddImageToPost(authorId, postId, imageUrl);
                return Created("", new { Message = "Image added successfully." });
            }
            catch (NotFound404Exception ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Forbidden403Exception ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }

        /// <summary>
        /// Редактирование существующего поста.
        /// </summary>
        /// <param name="postId">Guid поста.</param>
        /// <param name="dto">Модель данных для редактирования поста, включающая новый заголовок и новый контент (текст).</param>
        /// <returns>Результат (статус-код) с сообщением.</returns>
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [HttpPut("{postId}")]
        [Authorize(Roles = "Author")]
        public IActionResult EditPost(Guid postId, [FromBody] EditPostRequestModel dto)
        {
            try
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
                _postService.EditPost(userId, postId, dto.Title, dto.Content);
                return Ok(new { Message = "Post successfully updated." });
            }
            catch (NotFound404Exception ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Forbidden403Exception ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }

        /// <summary>
        /// Публикация существующего неопубликованного поста.
        /// </summary>
        /// <param name="postId">Guid поста.</param>
        /// <param name="dto">Модель данных для публикации поста, включающая новый статус поста.</param>
        /// <returns>Результат (статус-код) с сообщением.</returns>
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status409Conflict)] // В ТЗ нет, но, по-моему, должно быть
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [HttpPatch("{postId}/status")]
        [Authorize(Roles = "Author")]
        public IActionResult PublishPost(Guid postId, [FromBody] PublishPostRequestModel dto)
        {
            if (dto.Status != "Published")
                return BadRequest(new { Message = "Invalid status value." });
            try
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
                _postService.PublishPost(userId, postId);
                return Ok(new { Message = "Post successfully published." });
            }
            catch (NotFound404Exception ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Conflict409Exception ex)
            {
                return Conflict(new { ex.Message });
            }
            catch (Forbidden403Exception ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }

        /// <summary>
        /// Удаления изображения из поста.
        /// </summary>
        /// <param name="postId">Guid поста.</param>
        /// <param name="imageId">Guid изображения.</param>
        /// <returns>Результат (статус-код) с сообщением.</returns>
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [HttpDelete("{postId}/images/{imageId}")]
        [Authorize(Roles = "Author")]
        public IActionResult DeleteImage(Guid postId, Guid imageId)
        {
            try
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
                _postService.DeleteImageFromPost(userId, postId, imageId, _imageStoragePath);
                return Ok(new { Message = "Image successfully deleted." });
            }
            catch (NotFound404Exception ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Forbidden403Exception ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }

        /// <summary>
        /// Получение постов.
        /// </summary>
        /// <returns>Список постов.</returns>
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Authorize]
        public IActionResult GetPosts()
        {
            try
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
                var role = User.Claims.First(c => c.Type == ClaimTypes.Role).Value;

                IEnumerable<PostModel> posts;
                if (role == "Author")
                {
                    posts = _postService.GetAuthorPosts(userId);
                }
                else
                {
                    posts = _postService.GetPublishedPosts();
                }

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message });
            }
        }
    }
}
