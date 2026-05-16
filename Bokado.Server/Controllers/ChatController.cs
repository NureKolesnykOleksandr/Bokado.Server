using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Bokado.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : Controller
    {
        private readonly IChatRepository _chatRepository;

        public ChatController(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        [HttpGet("chats")]
        public async Task<ActionResult<List<ChatDto>>> GetUserChats()
        {
            try
            {
                int currentUserId = GetUserIdFromToken();
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
                int currentUserId = GetUserIdFromToken();
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
                if (message.Text == null) message.Text = "";
                int currentUserId = GetUserIdFromToken();
                var result = await _chatRepository.SendMessage(currentUserId, message);
                return result.Succeeded ? Ok(new { Message = "Message sent successfully" }) : BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{chatId}/read")]
        public async Task<IActionResult> MarkChatAsRead(int chatId)
        {
            try
            {
                int currentUserId = GetUserIdFromToken();
                await _chatRepository.MarkChatAsRead(chatId, currentUserId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("message/{messageId}")]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                int currentUserId = GetUserIdFromToken();
                var result = await _chatRepository.DeleteMessage(currentUserId, messageId);
                return result.Succeeded ? Ok(new { Message = "Message deleted successfully" }) : BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{chatId}")]
        public async Task<IActionResult> DeleteChat(int chatId)
        {
            try
            {
                int currentUserId = GetUserIdFromToken();
                var result = await _chatRepository.DeleteChat(currentUserId, chatId);
                return result.Succeeded ? Ok(new { Message = "Chat deleted successfully" }) : BadRequest(result.Errors);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
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
