using Bokado.Server.Dtos;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IFriendsRepository
    {
        Task<IdentityResult> SendFriendRequest(int fromId, int toId);
        Task<IdentityResult> AcceptFriendRequest(int currentUserId, int requesterId);
        Task<IdentityResult> DeclineFriendRequest(int currentUserId, int requesterId);
        Task<List<FriendRequestDto>> GetIncomingRequests(int currentUserId);
        Task<List<FriendRequestDto>> GetOutgoingRequests(int currentUserId);
        Task<List<FriendDto>> SearchUsersByUsername(int currentUserId, string query);
        Task<List<FriendDto>> SearchUsers(int currentUserId);
        Task<List<FriendDto>> GetTopUsers();
        Task<List<FriendDto>> GetFriends(int userId);
        Task<IdentityResult> RemoveFriend(int currentUserId, int friendId);
        Task<FriendStatusDto> GetFriendStatus(int currentUserId, int targetUserId);
    }
}
