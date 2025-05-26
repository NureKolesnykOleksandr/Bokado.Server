using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bokado.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FriendsController : ControllerBase
    {
        private readonly IFriendsRepository _friendsRepository;

        public FriendsController(IFriendsRepository friendsRepository)
        {
            _friendsRepository = friendsRepository;
        }

        [HttpPost("swipe/{targetUserId}")]
        public async Task<IActionResult> SwipeUser(int targetUserId, string action)
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            int currentUserId = GetUserIdFromToken(token);

            try
            {
                var result = await _friendsRepository.SwipeUser(currentUserId, targetUserId, action.ToLower());

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

        [HttpPost("accept/{swipeId}")]
        public async Task<IActionResult> AcceptFriendRequest(int swipeId)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int currentUserId = GetUserIdFromToken(token);
                var result = await _friendsRepository.AcceptFriendRequest(currentUserId, swipeId);

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

        [HttpGet("who-liked-me")]
        public async Task<IActionResult> GetUsersWhoLikedMe()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int currentUserId = GetUserIdFromToken(token);
                var users = await _friendsRepository.GetUsersWhoLikedMe(currentUserId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("top-users")]
        public async Task<IActionResult> GetTopUsers()
        {
            try
            {
                var users = await _friendsRepository.GetTopUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int currentUserId = GetUserIdFromToken(token);
                var users = await _friendsRepository.SearchUsers(currentUserId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("my-friends")]
        public async Task<IActionResult> GetFriends()
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int currentUserId = GetUserIdFromToken(token);
                var friends = await _friendsRepository.GetFriends(currentUserId);
                return Ok(friends);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("remove/{friendId}")]
        public async Task<IActionResult> RemoveFriend(int friendId)
        {
            try
            {
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                int currentUserId = GetUserIdFromToken(token);
                var result = await _friendsRepository.RemoveFriend(currentUserId, friendId);

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