using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Bokado.Server.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _adminRepository;
        private readonly ISubscribeRepository _subscribeRepository;
        private readonly IChatRepository _chatRepository;

        public AdminController(
            IAdminRepository adminRepository,
            ISubscribeRepository subscribeRepository,
            IChatRepository chatRepository)
        {
            _adminRepository = adminRepository;
            _subscribeRepository = subscribeRepository;
            _chatRepository = chatRepository;
        }

        [HttpPost("ban/{userId}")]
        public async Task<IActionResult> BanUser(int userId)
        {
            var result = await _adminRepository.BanUser(userId);
            return result.Succeeded
                ? Ok(new { Message = "User banned successfully" })
                : BadRequest(result.Errors);
        }

        [HttpPost("unban/{userId}")]
        public async Task<IActionResult> UnbanUser(int userId)
        {
            var result = await _adminRepository.UnbanUser(userId);
            return result.Succeeded
                ? Ok(new { Message = "User unbanned successfully" })
                : BadRequest(result.Errors);
        }

        [HttpPost("select-challenges")]
        public async Task<IActionResult> SelectChallenges([FromBody] List<int> challengeIds)
        {
            var result = await _adminRepository.SelectChallenges(challengeIds);
            return result.Succeeded
                ? Ok(new { Message = "Challenges updated successfully" })
                : BadRequest(result.Errors);
        }

        [HttpGet("allChallenges")]
        public async Task<IActionResult> GetAllChallenges()
        {
            var challenges = await _adminRepository.GetaAllChallenges();
            return Ok(challenges);
        }

        [HttpGet("allUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _adminRepository.GetaAllUsers();
            return Ok(users);
        }

        [HttpPut("subscribe")]
        public async Task<IActionResult> GiveSubscribe(int userId)
        {
            var result = await _subscribeRepository.GiveSubscribe(userId);
            return result.Succeeded
                ? Ok(new { Message = "User received subscribe" })
                : BadRequest(result.Errors);
        }

        [HttpDelete("subscribe")]
        public async Task<IActionResult> TakeSubscribe(int userId)
        {
            var result = await _subscribeRepository.TakeSubscribe(userId);
            return result.Succeeded
                ? Ok(new { Message = "User lost subscribe" })
                : BadRequest(result.Errors);
        }

        [HttpPost("warn/{userId}")]
        public async Task<IActionResult> WarnUser(int userId, [FromBody] WarnUserDto dto)
        {
            var result = await _adminRepository.WarnUser(userId, dto.Reason);
            return result.Succeeded
                ? Ok(new { Message = "Warning sent" })
                : BadRequest(result.Errors);
        }

        [HttpPost("support-chat/{userId}")]
        public async Task<IActionResult> OpenSupportChat(int userId)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var adminId = GetUserIdFromToken(token);
                var chat = await _chatRepository.CreateChat(adminId, userId);
                return Ok(chat);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private int GetUserIdFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new SecurityTokenException("User ID not found in token");
            return userId;
        }
    }
}