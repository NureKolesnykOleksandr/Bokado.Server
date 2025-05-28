using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Bokado.Server.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace Bokado.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChallengeController : ControllerBase
    {
        private readonly IChallengeRepository _challengeRepository;

        public ChallengeController(IChallengeRepository challengeRepository)
        {
            _challengeRepository = challengeRepository;
        }

        [Authorize]
        [HttpGet("challenges")]
        public async Task<ActionResult<List<Challenge>>> GetChallenges()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            int currentUserId = GetUserIdFromToken(token);
            var challenges = await _challengeRepository.GetChallenges(currentUserId);
            return Ok(challenges);
        }

        [Authorize]
        [HttpPost("check/{challengeId}")]
        public async Task<IActionResult> CheckChallenge(int challengeId)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            int currentUserId = GetUserIdFromToken(token);
            var result = await _challengeRepository.CheckChallenge(challengeId, currentUserId);
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

        [Authorize]
        [HttpPost("events")]
        public async Task<ActionResult<Event>> CreateEvent([FromBody] EventDto eventDto)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            int currentUserId = GetUserIdFromToken(token);
            var newEvent = await _challengeRepository.CreateEvent(eventDto, currentUserId);
            return CreatedAtAction(nameof(GetEvents), newEvent);
        }

        [Authorize]
        [HttpPost("events/join/{eventId}")]
        public async Task<IActionResult> JoinEvent(int eventId)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            int currentUserId = GetUserIdFromToken(token);
            var result = await _challengeRepository.JoinEvent(eventId, currentUserId);
            return result.Succeeded
                ? Ok(new { Message = "Successfully joined the event" })
                : BadRequest(result.Errors);
        }

        [Authorize]
        [HttpDelete("quit/{eventId}")]
        public async Task<IActionResult> QuitEvent(int eventId)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int currentUserId = GetUserIdFromToken(token);
                var result = await _challengeRepository.QuitEvent(eventId, currentUserId);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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