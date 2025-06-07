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

        private Dictionary<int, double> CalculateUserPriorities(User currentUser, List<User> users)
        {
            var priorities = new Dictionary<int, double>();

            var currentUserInterestIds = currentUser.UserInterests?
                .Select(ui => ui.InterestId)
                .ToHashSet() ?? new HashSet<int>();

            foreach (var user in users)
            {
                if (user.UserId == currentUser.UserId)
                    continue;

                double priorityScore = 0;

                // 1. Спільне місто (вага 30%)
                if (user.City == currentUser.City)
                {
                    priorityScore += 0.3;
                }

                // 2. Спільні інтереси (вага 40%)
                if (user.UserInterests != null && currentUserInterestIds.Any())
                {
                    int commonInterests = user.UserInterests
                        .Count(ui => currentUserInterestIds.Contains(ui.InterestId));

                    int maxPossibleCommon = Math.Min(
                        user.UserInterests.Count,
                        currentUserInterestIds.Count);

                    if (maxPossibleCommon > 0)
                    {
                        double interestMatchRatio = (double)commonInterests / maxPossibleCommon;
                        priorityScore += 0.4 * interestMatchRatio;
                    }
                }

                // 3. Активність користувача (вага 20%)
                // Чим новіша активність, тим вищий пріоритет
                double activityScore = 1 - (DateTime.UtcNow - user.LastActive).TotalDays / 30;
                activityScore = Math.Max(0, Math.Min(1, activityScore)); // Обмежуємо 0-1
                priorityScore += 0.2 * activityScore;

                // 4. Premium-статус (вага 20%)
                if (user.IsPremium)
                {
                    priorityScore += 0.2;
                }

                // 5. Різниця у віці +10% якщо різниця менше 10 років, ще +10% якщо менше 2
                if (Math.Abs((user.BirthDate - currentUser.BirthDate).TotalDays) < 3560)
                {
                    priorityScore += 0.1;

                    if (Math.Abs((user.BirthDate - currentUser.BirthDate).TotalDays) < 712)
                    {
                        priorityScore += 0.1;
                    }
                }

                priorities[user.UserId] = priorityScore;
            }

            return priorities;
        }

        public async Task<List<FriendDto>> SearchUsers(int currentUserId)
        {
            var random = new Random();

            User currentUser = await _context.Users.Where(u => u.UserId == currentUserId).Include(u=>u.UserInterests).FirstOrDefaultAsync();

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
            && DateTime.UtcNow-u.LastActive < TimeSpan.FromDays(30)
            && !u.IsBanned 
            && !u.IsAdmin
            && !friendIds.Contains(u.UserId)
            && !swiperIds.Contains(u.UserId)).Include(u => u.UserInterests).ToListAsync();

            List<FriendDto> userList = new List<FriendDto>();
            Dictionary<int, double> Priorities = CalculateUserPriorities(currentUser, users);


            var weightedUsers = users
                .Select(u => new
                {
                    User = u,
                    Weight = Priorities.GetValueOrDefault(u.UserId, 0.01)
                })
                .ToList();

            for (int i = 0; i < SearchResultsLimit &&  i < users.Count; i++)
            {
                double totalWeight = weightedUsers.Sum(w => w.Weight);

                double randomValue = random.NextDouble() * totalWeight;

                double cumulative = 0;
                foreach (var wu in weightedUsers)
                {
                    cumulative += wu.Weight;
                    if (cumulative >= randomValue)
                    {
                        userList.Add(new FriendDto
                        {
                            AvatarUrl = wu.User.AvatarUrl,
                            Bio = wu.User.Bio,
                            UserId = wu.User.UserId,
                            Username = wu.User.Username
                        });

                        weightedUsers.Remove(wu);
                        break;
                    }
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
