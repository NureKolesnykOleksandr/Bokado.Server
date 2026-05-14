using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IdentityResult> SendFriendRequest(int fromId, int toId)
        {
            if (fromId == toId)
                return IdentityResult.Failed(new IdentityError { Description = "Не можна надіслати запит собі" });

            if (!await _context.Users.AnyAsync(u => u.UserId == toId))
                return IdentityResult.Failed(new IdentityError { Description = "Користувача не знайдено" });

            if (await _context.Friendships.AnyAsync(f =>
                (f.UserId == fromId && f.FriendId == toId) ||
                (f.UserId == toId && f.FriendId == fromId)))
                return IdentityResult.Failed(new IdentityError { Description = "Ви вже друзі" });

            if (await _context.FriendRequests.AnyAsync(fr => fr.FromUserId == fromId && fr.ToUserId == toId))
                return IdentityResult.Failed(new IdentityError { Description = "Запит вже надісланий" });

            if (await _context.FriendRequests.AnyAsync(fr => fr.FromUserId == toId && fr.ToUserId == fromId))
                return IdentityResult.Failed(new IdentityError { Description = "Цей користувач вже надіслав вам запит" });

            await _context.FriendRequests.AddAsync(new FriendRequest
            {
                FromUserId = fromId,
                ToUserId = toId,
                SentAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AcceptFriendRequest(int currentUserId, int requesterId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.FromUserId == requesterId && fr.ToUserId == currentUserId);

            if (request == null)
                return IdentityResult.Failed(new IdentityError { Description = "Запит на дружбу не знайдено" });

            if (await _context.Friendships.AnyAsync(f =>
                (f.UserId == currentUserId && f.FriendId == requesterId) ||
                (f.UserId == requesterId && f.FriendId == currentUserId)))
                return IdentityResult.Failed(new IdentityError { Description = "Ви вже друзі" });

            _context.Friendships.Add(new Friendship
            {
                UserId = Math.Min(currentUserId, requesterId),
                FriendId = Math.Max(currentUserId, requesterId),
                CreatedAt = DateTime.UtcNow
            });

            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeclineFriendRequest(int currentUserId, int requesterId)
        {
            var request = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.FromUserId == requesterId && fr.ToUserId == currentUserId);

            if (request == null)
                return IdentityResult.Failed(new IdentityError { Description = "Запит на дружбу не знайдено" });

            _context.FriendRequests.Remove(request);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<List<FriendRequestDto>> GetIncomingRequests(int currentUserId)
        {
            return await _context.FriendRequests
                .Where(fr => fr.ToUserId == currentUserId)
                .Include(fr => fr.FromUser)
                .Select(fr => new FriendRequestDto
                {
                    FriendRequestId = fr.FriendRequestId,
                    UserId = fr.FromUser.UserId,
                    Username = fr.FromUser.Username,
                    AvatarUrl = fr.FromUser.AvatarUrl,
                    Bio = fr.FromUser.Bio,
                    City = fr.FromUser.City,
                    SentAt = fr.SentAt
                })
                .ToListAsync();
        }

        public async Task<List<FriendRequestDto>> GetOutgoingRequests(int currentUserId)
        {
            return await _context.FriendRequests
                .Where(fr => fr.FromUserId == currentUserId)
                .Include(fr => fr.ToUser)
                .Select(fr => new FriendRequestDto
                {
                    FriendRequestId = fr.FriendRequestId,
                    UserId = fr.ToUser.UserId,
                    Username = fr.ToUser.Username,
                    AvatarUrl = fr.ToUser.AvatarUrl,
                    Bio = fr.ToUser.Bio,
                    City = fr.ToUser.City,
                    SentAt = fr.SentAt
                })
                .ToListAsync();
        }

        public async Task<List<FriendDto>> SearchUsersByUsername(int currentUserId, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<FriendDto>();

            return await _context.Users
                .Where(u =>
                    u.UserId != currentUserId &&
                    !u.IsBanned &&
                    !u.IsAdmin &&
                    u.Username.ToLower().Contains(query.ToLower()))
                .Select(u => new FriendDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    AvatarUrl = u.AvatarUrl,
                    Bio = u.Bio,
                    City = u.City
                })
                .Take(20)
                .ToListAsync();
        }

        public async Task<List<FriendDto>> SearchUsers(int currentUserId)
        {
            var random = new Random();

            var currentUser = await _context.Users
                .Where(u => u.UserId == currentUserId)
                .Include(u => u.UserInterests)
                .FirstOrDefaultAsync();

            var friendIds = await _context.Friendships
                .Where(f => f.UserId == currentUserId || f.FriendId == currentUserId)
                .Select(f => f.UserId == currentUserId ? f.FriendId : f.UserId)
                .ToListAsync();

            var pendingIds = await _context.FriendRequests
                .Where(fr => fr.FromUserId == currentUserId || fr.ToUserId == currentUserId)
                .Select(fr => fr.FromUserId == currentUserId ? fr.ToUserId : fr.FromUserId)
                .ToListAsync();

            var users = await _context.Users
                .Where(u =>
                    u.UserId != currentUserId &&
                    DateTime.UtcNow - u.LastActive < TimeSpan.FromDays(30) &&
                    !u.IsBanned &&
                    !u.IsAdmin &&
                    !friendIds.Contains(u.UserId) &&
                    !pendingIds.Contains(u.UserId))
                .Include(u => u.UserInterests)
                .ToListAsync();

            var priorities = CalculateUserPriorities(currentUser, users);

            var weightedUsers = users
                .Select(u => new { User = u, Weight = priorities.GetValueOrDefault(u.UserId, 0.01) })
                .ToList();

            var result = new List<FriendDto>();
            for (int i = 0; i < SearchResultsLimit && weightedUsers.Any(); i++)
            {
                double total = weightedUsers.Sum(w => w.Weight);
                double rand = random.NextDouble() * total;
                double cumulative = 0;
                foreach (var wu in weightedUsers)
                {
                    cumulative += wu.Weight;
                    if (cumulative >= rand)
                    {
                        result.Add(new FriendDto
                        {
                            UserId = wu.User.UserId,
                            Username = wu.User.Username,
                            AvatarUrl = wu.User.AvatarUrl,
                            Bio = wu.User.Bio,
                            City = wu.User.City
                        });
                        weightedUsers.Remove(wu);
                        break;
                    }
                }
            }
            return result;
        }

        public async Task<List<FriendDto>> GetFriends(int userId)
        {
            return await _context.Friendships
                .Where(f => f.UserId == userId || f.FriendId == userId)
                .Select(f => new FriendDto
                {
                    UserId = f.UserId == userId ? f.FriendId : f.UserId,
                    Username = f.UserId == userId ? f.Friend.Username : f.User.Username,
                    AvatarUrl = f.UserId == userId ? f.Friend.AvatarUrl : f.User.AvatarUrl,
                    Bio = f.UserId == userId ? f.Friend.Bio : f.User.Bio,
                    City = f.UserId == userId ? f.Friend.City : f.User.City
                })
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

        public async Task<List<FriendDto>> GetTopUsers()
        {
            return await _context.Users
                .Where(u => !u.IsAdmin && !u.IsBanned)
                .OrderByDescending(u => u.Level)
                .Take(10)
                .Select(u => new FriendDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    AvatarUrl = u.AvatarUrl,
                    Bio = u.Bio,
                    City = u.City
                })
                .ToListAsync();
        }

        public async Task<FriendStatusDto> GetFriendStatus(int currentUserId, int targetUserId)
        {
            bool areFriends = await _context.Friendships.AnyAsync(f =>
                (f.UserId == currentUserId && f.FriendId == targetUserId) ||
                (f.UserId == targetUserId && f.FriendId == currentUserId));

            if (areFriends) return new FriendStatusDto { Status = "friends" };

            bool pendingSent = await _context.FriendRequests.AnyAsync(fr =>
                fr.FromUserId == currentUserId && fr.ToUserId == targetUserId);

            if (pendingSent) return new FriendStatusDto { Status = "pending_sent" };

            bool pendingReceived = await _context.FriendRequests.AnyAsync(fr =>
                fr.FromUserId == targetUserId && fr.ToUserId == currentUserId);

            if (pendingReceived) return new FriendStatusDto { Status = "pending_received" };

            return new FriendStatusDto { Status = "none" };
        }

        private Dictionary<int, double> CalculateUserPriorities(User currentUser, List<User> users)
        {
            var priorities = new Dictionary<int, double>();
            var currentInterestIds = currentUser?.UserInterests?
                .Select(ui => ui.InterestId).ToHashSet() ?? new HashSet<int>();

            foreach (var user in users)
            {
                double score = 0;

                if (user.City == currentUser?.City) score += 0.3;

                if (user.UserInterests != null && currentInterestIds.Any())
                {
                    int common = user.UserInterests.Count(ui => currentInterestIds.Contains(ui.InterestId));
                    int maxPossible = Math.Min(user.UserInterests.Count, currentInterestIds.Count);
                    if (maxPossible > 0) score += 0.4 * ((double)common / maxPossible);
                }

                double activityScore = 1 - (DateTime.UtcNow - user.LastActive).TotalDays / 30;
                score += 0.2 * Math.Max(0, Math.Min(1, activityScore));

                if (user.IsPremium) score += 0.2;

                if (currentUser != null)
                {
                    double ageDiff = Math.Abs((user.BirthDate - currentUser.BirthDate).TotalDays);
                    if (ageDiff < 3560) score += 0.1;
                    if (ageDiff < 712) score += 0.1;
                }

                priorities[user.UserId] = score;
            }
            return priorities;
        }
    }
}
