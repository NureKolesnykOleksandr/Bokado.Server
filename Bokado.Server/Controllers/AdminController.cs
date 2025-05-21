using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bokado.Server.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _adminRepository;

        public AdminController(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
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

        [HttpGet("stats")]
        public async Task<IActionResult> GetPlatformStats()
        {
            return Ok(new
            {
                TotalUsers = 0,
                ActiveUsers = 0,
                BannedUsers = 0,
                TotalChallenges = 0
            });
        }

        [HttpGet("allChallenges")]
        public async Task<IActionResult> GetAllChallenges()
        {
            var challenges = await _adminRepository.GetaAllChallenges();
            return Ok(challenges);
        }
    }
}