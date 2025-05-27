using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bokado.Server.Repositories
{
    public class FriendsRepository : IFriendsRepository
    {
        private readonly SocialNetworkContext _context;
        private const int SearchResultsLimit = 5;

        public FriendsRepository(SocialNetworkContext context)
        {
            _context = context;
        }

        public async Task<IdentityResult> AcceptFriendRequest(int currentUserId, int swipeId)
        {
            var swipe = await _context.Swipes.FirstOrDefaultAsync(s => s.SwipeId == swipeId);

            if (swipe == null)
                return IdentityResult.Failed(new IdentityError { Description = "Такого свайпу не існує" });

            bool areFriends = await _context.Friendships
                .AnyAsync(f =>
                    (f.UserId == currentUserId && f.FriendId == swipe.SwiperId) ||
                    (f.UserId == swipe.SwiperId && f.FriendId == currentUserId));

            if (areFriends)
                return IdentityResult.Failed(new IdentityError { Description = "Ви вже друзі з цим користувачем" });

            var friendship = new Friendship
            {
                UserId = Math.Min(currentUserId, swipe.SwiperId),
                FriendId = Math.Max(currentUserId, swipe.SwiperId),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Friendships.AddAsync(friendship);
            _context.Swipes.Remove(swipe);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<List<UserSwipeDto>> GetUsersWhoLikedMe(int currentUserId)
        {
            return await _context.Swipes
                .Where(s =>
                    s.TargetUserId == currentUserId &&
                    s.Action == "like" &&
                    !_context.Swipes.Any(s2 =>
                        s2.SwiperId == currentUserId &&
                        s2.TargetUserId == s.SwiperId) &&
                    !_context.Friendships.Any(f =>
                        (f.UserId == currentUserId && f.FriendId == s.SwiperId) ||
                        (f.UserId == s.SwiperId && f.FriendId == currentUserId)))
                .Select(s => new UserSwipeDto
                {
                    SwipeId = s.SwipeId,
                    UserId = s.SwiperId,
                    Username = s.Swiper.Username,
                    AvatarUrl = s.Swiper.AvatarUrl,
                    Bio = s.Swiper.Bio,
                    SwipedAt = s.SwipedAt,
                    Action = s.Action
                })
                .ToListAsync();
        }

        public async Task<List<UserSwipeDto>> GetMySwipes(int currentUserId)
        {
            return await _context.Swipes
                .Where(s =>
                    s.SwiperId == currentUserId )
                .Select(s => new UserSwipeDto
                {
                    SwipeId = s.SwipeId,
                    UserId = s.TargetUserId,
                    Username = s.TargetUser.Username,
                    AvatarUrl = s.TargetUser.AvatarUrl,
                    Bio = s.TargetUser.Bio,
                    SwipedAt = s.SwipedAt,
                    Action = s.Action
                })
                .ToListAsync();
        }

        public async Task<List<FriendDto>> SearchUsers(int currentUserId)
        {
            var random = new Random();

            var friendIds = await _context.Friendships
            .Where(f => f.UserId == currentUserId || f.FriendId == currentUserId)
            .Select(f => f.UserId == currentUserId ? f.FriendId : f.UserId) 
            .Distinct()
            .ToListAsync();

            var swiperIds = await _context.Swipes
                .Where(f => f.SwiperId == currentUserId || f.TargetUserId == currentUserId)
                .Select(f => f.SwiperId == currentUserId ? f.TargetUserId : f.SwiperId)
                .Distinct()
                .ToListAsync();

            var users = await _context.Users.Where(u=> 
            u.UserId!=currentUserId 
            && !u.IsBanned 
            && !u.IsAdmin
            && !friendIds.Contains(u.UserId)
            && !swiperIds.Contains(u.UserId)).ToListAsync();

            List<FriendDto> userList = new List<FriendDto>();

            for (int i = 0; i < SearchResultsLimit &&  i < users.Count; i++)
            {
                User user = users[random.Next(0, users.Count)];
                if (!userList.Select(u=>u.UserId).Contains(user.UserId))
                {
                    userList.Add(new FriendDto { AvatarUrl = user.AvatarUrl, Bio = user.Bio, UserId = user.UserId, Username = user.Username});
                }
            }
            return userList;
        }

        public async Task<IdentityResult> SwipeUser(int swiperId, int targetUserId, string action)
        {
            if (action != "like" && action != "pass")
                return IdentityResult.Failed(new IdentityError { Description = "Невірна дія. Дозволені лише 'like' або 'pass'" });

            if (await _context.Swipes.AnyAsync(s =>
                s.SwiperId == swiperId && s.TargetUserId == targetUserId))
                return IdentityResult.Failed(new IdentityError { Description = "Такий свайп вже існує" });

            var swipe = new Swipe
            {
                SwiperId = swiperId,
                TargetUserId = targetUserId,
                Action = action,
                SwipedAt = DateTime.UtcNow
            };

            if (action == "like")
            {
                var mutualLike = await _context.Swipes
                    .AnyAsync(s =>
                        s.SwiperId == targetUserId &&
                        s.TargetUserId == swiperId &&
                        s.Action == "like");

                swipe.IsMatch = mutualLike;

                if (mutualLike)
                {
                    var friendship = new Friendship
                    {
                        UserId = Math.Min(swiperId, targetUserId),
                        FriendId = Math.Max(swiperId, targetUserId),
                        CreatedAt = DateTime.UtcNow
                    };

                    await _context.Friendships.AddAsync(friendship);
                }
            }

            await _context.Swipes.AddAsync(swipe);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<List<FriendDto>> GetFriends(int userId)
        {
            return await _context.Friendships
                .Where(f =>
                    f.UserId == userId ||
                    f.FriendId == userId)
                .Select(f => new FriendDto
                {
                    UserId = f.UserId == userId ? f.FriendId : f.UserId,
                    Username = f.UserId == userId ? f.Friend.Username : f.User.Username,
                    AvatarUrl = f.UserId == userId ? f.Friend.AvatarUrl : f.User.AvatarUrl,
                    Bio = f.UserId == userId ? f.Friend.Bio : f.User.Bio
                })
                .Distinct()
                .ToListAsync();
        }

        public async Task<IdentityResult> RemoveFriend(int currentUserId, int friendId)
        {
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.UserId == currentUserId && f.FriendId == friendId) ||
                    (f.UserId == friendId && f.FriendId == currentUserId));

            if (friendship == null)
                return IdentityResult.Failed(new IdentityError { Description = "Дружба не знайдена" });

            _context.Friendships.Remove(friendship);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<List<User>> GetTopUsers()
        {
            return await _context.Users
                .Where(u=>!u.IsAdmin)
                .OrderByDescending(u => u.Level)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IdentityResult> RemoveSwipe(int currentUserId, int swipeId)
        {
            User? user = await _context.Users.Where(u => u.UserId == currentUserId).FirstOrDefaultAsync();
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Щось пішло не так" });
            }
            Swipe? swipe = await _context.Swipes.Where(s => s.SwipeId == swipeId).FirstOrDefaultAsync();
            if(swipe== null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Цей свайп не було знайдено" });
            }
            if(swipe.TargetUserId != currentUserId && !user.IsAdmin && swipe.SwiperId!=currentUserId)
            {
                return IdentityResult.Failed(new IdentityError { Description = "У вас немає права видаляти цей свайп" });
            }

            _context.Swipes.Remove(swipe);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
    }
}