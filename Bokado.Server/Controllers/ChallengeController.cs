using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bokado.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengeController : ControllerBase
    {
        private readonly IChallengeRepository _challengeRepository;

        public ChallengeController(IChallengeRepository challengeRepository)
        {
            _challengeRepository = challengeRepository;
        }

        [HttpGet("challenges")]
        public async Task<ActionResult<List<Challenge>>> GetChallenges()
        {
            var challenges = await _challengeRepository.GetChallenges();
            return Ok(challenges);
        }

        [HttpPost("check/{challengeId}/{userId}")]
        public async Task<IActionResult> CheckChallenge(int challengeId, int userId)
        {
            var result = await _challengeRepository.CheckChallenge(challengeId, userId);
            return result.Succeeded
                ? Ok(new { Message = "Challenge completed successfully" })
                : BadRequest(result.Errors);
        }

        [HttpGet("events")]
        public async Task<ActionResult<List<Event>>> GetEvents()
        {
            var events = await _challengeRepository.GetEvents();
            return Ok(events);
        }

        [HttpPost("events")]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] EventDto eventDto, int creatorId)
        {
            var newEvent = await _challengeRepository.CreateEvent(eventDto, creatorId);
            return CreatedAtAction(nameof(GetEvents), newEvent);
        }

        [HttpPost("events/join/{eventId}/{userId}")]
        public async Task<IActionResult> JoinEvent(int eventId, int userId)
        {
            var result = await _challengeRepository.JoinEvent(eventId, userId);
            return result.Succeeded
                ? Ok(new { Message = "Successfully joined the event" })
                : BadRequest(result.Errors);
        }
    }
}