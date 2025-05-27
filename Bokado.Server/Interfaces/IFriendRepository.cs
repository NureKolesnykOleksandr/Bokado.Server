using Bokado.Server.Dtos;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bokado.Server.Interfaces
{
    public interface IFriendsRepository
    {
        Task<IdentityResult> AcceptFriendRequest(int currentUserId, int swipeId);
        Task<List<UserSwipeDto>> GetUsersWhoLikedMe(int currentUserId);
        Task<List<UserSwipeDto>> GetMySwipes(int currentUserId);
        Task<List<User>> GetTopUsers();
        Task<List<FriendDto>> SearchUsers(int currentUserId);
        Task<IdentityResult> SwipeUser(int swiperId, int targetUserId, string action);
        Task<List<FriendDto>> GetFriends(int userId);
        Task<IdentityResult> RemoveFriend(int currentUserId, int friendId);
        Task<IdentityResult> RemoveSwipe(int currentUserId ,int swipeId);
    }
}