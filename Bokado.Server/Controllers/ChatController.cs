using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;

namespace Bokado.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : Controller
    {
        IChatRepository _chatRepository;

        public ChatController(IChatRepository chatRepository) 
        {
            _chatRepository = chatRepository;
        }

        [HttpGet("chats")]
        public async Task<ActionResult<List<ChatDto>>> GetUserChats()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int currentUserId = GetUserIdFromToken(token);
                var chats = await _chatRepository.GetChats(currentUserId);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{chatId}/messages")]
        public async Task<ActionResult> GetChatMessages(int chatId)
        {
            try
            {
                var messages = await _chatRepository.GetMessages(chatId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateChat(int withUserId)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int currentUserId = GetUserIdFromToken(token);
                var result = await _chatRepository.CreateChat(currentUserId, withUserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromForm] MessageDto message)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int currentUserId = GetUserIdFromToken(token);
                var result = await _chatRepository.SendMessage(currentUserId, message);

                if (result.Succeeded)
                {
                    return Ok(new { Message = "Message sent successfully" });
                }

                return BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int currentUserId = GetUserIdFromToken(token);
                var result = await _chatRepository.DeleteMessage(currentUserId, messageId);

                if (result.Succeeded)
                {
                    return Ok(new { Message = "Message deleted successfully" });
                }

                return BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private int GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    throw new SecurityTokenException("User ID not found in token or invalid format");
                }


                return userId;
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Invalid token format", ex);
            }
        }
    }
}
