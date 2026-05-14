using Bokado.Server.Dtos;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IGroupRepository
    {
        Task<List<GetGroupDto>> GetGroups();
        Task<GetGroupDto?> GetGroup(int groupId);
        Task<GetGroupDto> CreateGroup(CreateGroupDto dto, int creatorId);
        Task<IdentityResult> UpdateGroup(int groupId, int userId, UpdateGroupDto dto);
        Task<IdentityResult> DeleteGroup(int groupId, int userId);
        Task<IdentityResult> CloseGroup(int groupId, int userId);
        Task<IdentityResult> JoinGroup(int groupId, int userId);
        Task<IdentityResult> LeaveGroup(int groupId, int userId);
        Task<IdentityResult> KickMember(int groupId, int requesterId, int targetUserId);
        Task<IdentityResult> AssignAdmin(int groupId, int requesterId, int targetUserId);
        Task<List<GetGroupDto>> GetRecommendations(int userId);
        Task<string> StartGroupCall(int groupId, int userId);
    }
}
