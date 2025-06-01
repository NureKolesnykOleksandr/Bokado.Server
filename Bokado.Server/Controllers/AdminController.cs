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
        private readonly IStatisticRepository _statisticRepository;
        private readonly ISubscribeRepository _subscribeRepository;

        public AdminController(IAdminRepository adminRepository, IStatisticRepository statisticRepository, ISubscribeRepository subscribeRepository)
        {
            _adminRepository = adminRepository;
            _statisticRepository = statisticRepository;
            _subscribeRepository = subscribeRepository;
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

        [HttpGet("stats/Users")]
        public async Task<IActionResult> GetUsersPerMonth()
        {
            var usersPerMonth = await _statisticRepository.GetUsersPerMonth();
            return Ok(usersPerMonth);
        }

        [HttpGet("stats/Challenges")]
        public async Task<IActionResult> GetChallengesCompleted()
        {
            var completedChallenges = await _statisticRepository.GetChallengesCompleted();
            return Ok(completedChallenges);
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
    }
}