using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Chat>>> GetUserChats(int userId)
        {
            try
            {
                var chats = await _chatRepository.GetChats(userId);
                return Ok(chats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{chatId}/messages")]
        public async Task<ActionResult<List<Message>>> GetChatMessages(int chatId)
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

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageDto message)
        {

            try
            {
                var result = await _chatRepository.SendMessage(message);

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
    }
}
