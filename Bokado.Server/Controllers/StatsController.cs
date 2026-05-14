using Bokado.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bokado.Server.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/stats")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly IStatisticRepository _statisticRepository;

        public StatsController(IStatisticRepository statisticRepository)
        {
            _statisticRepository = statisticRepository;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var overview = await _statisticRepository.GetOverview();
            return Ok(overview);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsersPerMonth()
        {
            var data = await _statisticRepository.GetUsersPerMonth();
            return Ok(data);
        }

        [HttpGet("challenges")]
        public async Task<IActionResult> GetChallengesCompleted()
        {
            var data = await _statisticRepository.GetChallengesCompleted();
            return Ok(data);
        }
    }
}
