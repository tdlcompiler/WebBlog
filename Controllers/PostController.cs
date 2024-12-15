namespace WebBlog.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using WebBlog.Models;
    using WebBlog.Models.Requests;
    using WebBlog.Services;

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

        [HttpPost]
        [Authorize(Roles = "Author")]
        public IActionResult CreatePost([FromBody] CreatePostRequestModel dto)
        {
            try
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == "sub").Value);
                var post = _postService.CreatePost(userId, dto.Title, dto.Content, dto.IdempotencyKey);
                return CreatedAtAction(nameof(GetPost), new { id = post.PostId }, post);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
        }

        [HttpPost("{postId}/images")]
        [Authorize(Roles = "Author")]
        [Consumes("multipart/form-data")]
        public IActionResult AddImageToPost(Guid postId, [FromForm] IFormFile image)
        {
            if (image == null) return BadRequest(new { Message = "No file uploaded." });

            var imageUrl = SaveImageToFileSystem(image);
            _postService.AddImageToPost(postId, imageUrl);
            return Created("", new { Message = "Image added successfully." });
        }

        [HttpPut("{postId}")]
        [Authorize(Roles = "Author")]
        public IActionResult EditPost(Guid postId, [FromBody] EditPostRequestModel dto)
        {
            try
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == "sub").Value);
                _postService.EditPost(userId, postId, dto.Title, dto.Content);
                return Ok(new { Message = "Post successfully updated." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpPatch("{postId}/status")]
        [Authorize(Roles = "Author")]
        public IActionResult PublishPost(Guid postId, [FromBody] PublishPostRequestModel dto)
        {
            if (dto.Status != "Published")
                return BadRequest(new { Message = "Invalid status value." });

            try
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == "sub").Value);
                _postService.PublishPost(userId, postId);
                return Ok(new { Message = "Post successfully published." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("{postId}/images/{imageId}")]
        [Authorize(Roles = "Author")]
        public IActionResult DeleteImage(Guid postId, Guid imageId)
        {
            try
            {
                var userId = Guid.Parse(User.Claims.First(c => c.Type == "sub").Value);
                _postService.DeleteImageFromPost(userId, postId, imageId);
                return Ok(new { Message = "Image successfully deleted." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [Authorize(Roles = "Author")]
        [HttpGet("{postId}")]
        public IActionResult GetPost([FromRoute] Guid postId)
        {
            var post = _postService.GetPost(postId);

            if (post == null)
                return NotFound("Post not found.");

            return Ok(post);
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetPosts()
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "sub").Value);
            var role = User.Claims.First(c => c.Type == "role").Value;

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

        private string SaveImageToFileSystem(IFormFile image)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var filePath = Path.Combine(_imageStoragePath, fileName);

            Directory.CreateDirectory(_imageStoragePath);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(stream);
            }

            return $"/images/{fileName}";
        }
    }
}
