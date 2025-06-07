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
    public class EventController : ControllerBase
    {
        private readonly IEventRepository _challengeRepository;

        public EventController(IEventRepository challengeRepository)
        {
            _challengeRepository = challengeRepository;
        }

        [HttpGet("events")]
        public async Task<ActionResult> GetEvents()
        {
            var events = await _challengeRepository.GetEvents();
            return Ok(events);
        }

        [Authorize]
        [HttpPost("events")]
        public async Task<ActionResult> CreateEvent([FromBody] CreateEventDto eventDto)
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

        [Authorize]
        [HttpPut("{eventId}")]
        public async Task<IActionResult> UpdateEvent(int eventId, UpdateEventDto eventDto)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int currentUserId = GetUserIdFromToken(token);
                var result = await _challengeRepository.UpdateEvent(eventId, currentUserId, eventDto);

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