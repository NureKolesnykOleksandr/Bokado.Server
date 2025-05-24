using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Bokado.Server.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProfile(int userId)
        {
            try
            {
                var profile = await _userRepository.GetUserProfile(userId);
                return Ok(profile);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("GetDetail/{userId}")]
        public async Task<IActionResult> GetDetailedUser(int userId)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                var tuple = GetUserIdAndRoleFromToken(token);

                if (tuple.userId != userId && tuple.role != "Admin")
                {
                    return Forbid();
                }

                var user = await _userRepository.GetDetailedUserInfo(userId);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private (int userId, string role) GetUserIdAndRoleFromToken(string token)
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

                var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "role");
                if (roleClaim == null)
                {
                    throw new SecurityTokenException("Role not found in token");
                }

                return (userId, roleClaim.Value);
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Invalid token format", ex);
            }
        }
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateProfile(int userId, [FromForm] UpdateUserDto user)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                await _userRepository.UpdateUserProfile(userId, user);
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}