using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Bokado.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _postRepository;

        public PostController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPosts(int userId)
        {
            var currentUserId = GetUserIdFromToken();
            var posts = await _postRepository.GetUserPosts(userId, currentUserId);
            return Ok(posts);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto dto)
        {
            var userId = GetUserIdFromToken();
            var post = await _postRepository.CreatePost(dto, userId);
            return CreatedAtAction(nameof(GetUserPosts), new { userId }, post);
        }

        [Authorize]
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var userId = GetUserIdFromToken();
            var result = await _postRepository.DeletePost(postId, userId);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [Authorize]
        [HttpPost("{postId}/like")]
        public async Task<IActionResult> LikePost(int postId)
        {
            var userId = GetUserIdFromToken();
            var result = await _postRepository.LikePost(postId, userId);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [Authorize]
        [HttpDelete("{postId}/like")]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            var userId = GetUserIdFromToken();
            var result = await _postRepository.UnlikePost(postId, userId);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        private int GetUserIdFromToken()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new SecurityTokenException("User ID not found in token");
            return userId;
        }
    }
}
