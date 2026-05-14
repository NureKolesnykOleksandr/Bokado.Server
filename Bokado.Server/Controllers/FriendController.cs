using Bokado.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

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

        [HttpPost("request/{targetUserId}")]
        public async Task<IActionResult> SendFriendRequest(int targetUserId)
        {
            try
            {
                var result = await _friendsRepository.SendFriendRequest(GetUserId(), targetUserId);
                return result.Succeeded ? Ok() : BadRequest(result.Errors);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("requests/incoming")]
        public async Task<IActionResult> GetIncomingRequests()
        {
            var requests = await _friendsRepository.GetIncomingRequests(GetUserId());
            return Ok(requests);
        }

        [HttpGet("requests/outgoing")]
        public async Task<IActionResult> GetOutgoingRequests()
        {
            var requests = await _friendsRepository.GetOutgoingRequests(GetUserId());
            return Ok(requests);
        }

        [HttpPost("request/accept/{requesterId}")]
        public async Task<IActionResult> AcceptFriendRequest(int requesterId)
        {
            try
            {
                var result = await _friendsRepository.AcceptFriendRequest(GetUserId(), requesterId);
                return result.Succeeded ? Ok() : BadRequest(result.Errors);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpDelete("request/decline/{requesterId}")]
        public async Task<IActionResult> DeclineFriendRequest(int requesterId)
        {
            try
            {
                var result = await _friendsRepository.DeclineFriendRequest(GetUserId(), requesterId);
                return result.Succeeded ? Ok() : BadRequest(result.Errors);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("search/username")]
        public async Task<IActionResult> SearchByUsername([FromQuery] string query)
        {
            var users = await _friendsRepository.SearchUsersByUsername(GetUserId(), query);
            return Ok(users);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers()
        {
            var users = await _friendsRepository.SearchUsers(GetUserId());
            return Ok(users);
        }

        [HttpGet("top-users")]
        public async Task<IActionResult> GetTopUsers()
        {
            var users = await _friendsRepository.GetTopUsers();
            return Ok(users);
        }

        [HttpGet("my-friends")]
        public async Task<IActionResult> GetFriends()
        {
            var friends = await _friendsRepository.GetFriends(GetUserId());
            return Ok(friends);
        }

        [HttpDelete("remove/{friendId}")]
        public async Task<IActionResult> RemoveFriend(int friendId)
        {
            try
            {
                var result = await _friendsRepository.RemoveFriend(GetUserId(), friendId);
                return result.Succeeded ? Ok() : BadRequest(result.Errors);
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet("status/{targetUserId}")]
        public async Task<IActionResult> GetFriendStatus(int targetUserId)
        {
            var status = await _friendsRepository.GetFriendStatus(GetUserId(), targetUserId);
            return Ok(status);
        }

        private int GetUserId()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (claim == null || !int.TryParse(claim.Value, out int userId))
                throw new SecurityTokenException("User ID not found in token");
            return userId;
        }
    }
}
