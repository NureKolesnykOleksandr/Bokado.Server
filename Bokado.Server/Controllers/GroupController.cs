using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Bokado.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupRepository _groupRepository;

        public GroupController(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _groupRepository.GetGroups();
            return Ok(groups);
        }

        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroup(int groupId)
        {
            var group = await _groupRepository.GetGroup(groupId);
            if (group == null)
                return NotFound();
            return Ok(group);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var group = await _groupRepository.CreateGroup(dto, userId);
                return CreatedAtAction(nameof(GetGroup), new { groupId = group.GroupId }, group);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        [Authorize]
        [HttpPut("{groupId}")]
        public async Task<IActionResult> UpdateGroup(int groupId, [FromBody] UpdateGroupDto dto)
        {
            var userId = GetUserIdFromToken();
            var result = await _groupRepository.UpdateGroup(groupId, userId, dto);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [Authorize]
        [HttpDelete("{groupId}")]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            var userId = GetUserIdFromToken();
            var result = await _groupRepository.DeleteGroup(groupId, userId);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [Authorize]
        [HttpPost("{groupId}/close")]
        public async Task<IActionResult> CloseGroup(int groupId)
        {
            var userId = GetUserIdFromToken();
            var result = await _groupRepository.CloseGroup(groupId, userId);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [Authorize]
        [HttpPost("{groupId}/join")]
        public async Task<IActionResult> JoinGroup(int groupId)
        {
            var userId = GetUserIdFromToken();
            var result = await _groupRepository.JoinGroup(groupId, userId);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [Authorize]
        [HttpPost("{groupId}/leave")]
        public async Task<IActionResult> LeaveGroup(int groupId)
        {
            var userId = GetUserIdFromToken();
            var result = await _groupRepository.LeaveGroup(groupId, userId);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [Authorize]
        [HttpDelete("{groupId}/kick/{targetUserId}")]
        public async Task<IActionResult> KickMember(int groupId, int targetUserId)
        {
            var requesterId = GetUserIdFromToken();
            var result = await _groupRepository.KickMember(groupId, requesterId, targetUserId);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [Authorize]
        [HttpPut("{groupId}/admin/{targetUserId}")]
        public async Task<IActionResult> AssignAdmin(int groupId, int targetUserId)
        {
            var requesterId = GetUserIdFromToken();
            var result = await _groupRepository.AssignAdmin(groupId, requesterId, targetUserId);
            return result.Succeeded ? Ok() : BadRequest(result.Errors);
        }

        [Authorize]
        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations()
        {
            var userId = GetUserIdFromToken();
            var groups = await _groupRepository.GetRecommendations(userId);
            return Ok(groups);
        }

        [Authorize]
        [HttpPost("{groupId}/call")]
        public async Task<IActionResult> StartGroupCall(int groupId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var link = await _groupRepository.StartGroupCall(groupId, userId);
                return Ok(new { meetLink = link });
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException ex) { return Forbid(); }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        private int GetUserIdFromToken()
        {
            var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                throw new SecurityTokenException("User ID not found in token");
            return userId;
        }
    }
}
