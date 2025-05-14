using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
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

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        }

        [HttpPost("swipe/{targetUserId}/{action}")]
        public async Task<IActionResult> SwipeUser(int targetUserId, string action)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
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
                var currentUserId = GetCurrentUserId();
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
                var currentUserId = GetCurrentUserId();
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
                var currentUserId = GetCurrentUserId();
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
                var currentUserId = GetCurrentUserId();
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
                var currentUserId = GetCurrentUserId();
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
    }
}