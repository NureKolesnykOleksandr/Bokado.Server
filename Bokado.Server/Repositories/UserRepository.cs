using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Bokado.Server.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SocialNetworkContext _context;

        public UserRepository(SocialNetworkContext context)
        {
            _context = context;
        }

        public async Task<UserInfoDto> GetUserProfile(int userId)
        {
            User user = await _context.Users.Where(u=>u.UserId == userId).FirstOrDefaultAsync();
            return new UserInfoDto()
            {
                AvatarUrl = user.AvatarUrl,
                Status = user.Status,
                Bio = user.Bio,
                BirthDate = user.BirthDate,
                City = user.City,
                CreatedAt = user.CreatedAt,
                IsAdmin = user.IsAdmin,
                IsBanned = user.IsBanned, 
                IsPremium = user.IsPremium, 
                LastActive = user.LastActive, 
                Level = user.Level,
                UserId = userId,
                Username = user.Username
            };
        }

        public async Task<UserDetailInfoDto> GetDetailedUserInfo(int userId)
        {
            User? user = await _context.Users
                .Where(u => u.UserId == userId)
                .Include(u => u.UserInterests)
                .Include(u => u.Friends)
                .Include(u => u.Swipes)
                .Include(u => u.ChatParticipants)
                .Include(u => u.EventParticipants)
                .Include(u => u.UserChallenges)
                .Include(u => u.Messages)
                .Include(u => u.CreatedEvents)
                .FirstOrDefaultAsync();

            return new UserDetailInfoDto()
            {
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Status = user.Status,
                Bio = user.Bio,
                BirthDate = user.BirthDate,
                City = user.City,
                CreatedAt = user.CreatedAt,
                IsAdmin = user.IsAdmin,
                IsBanned = user.IsBanned,
                IsPremium = user.IsPremium,
                LastActive = user.LastActive,
                Level = user.Level,
                UserId = userId,
                Username = user.Username,
                Swipes = user.Swipes,
                ChatParticipants = user.ChatParticipants,
                CreatedEvents = user.CreatedEvents,
                EventParticipants = user.EventParticipants,
                Friends = user.Friends,
                Messages = user.Messages,
                UserChallenges = user.UserChallenges,
                UserInterests = user.UserInterests
                 
            };
        }

        public async Task UpdateUserProfile(int userId, User user)
        {
            var localUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (localUser == null)
            {
                throw new ArgumentException("User not found");
            }

            localUser.AvatarUrl = user.AvatarUrl;
            localUser.Status = user.Status;
            localUser.Bio = user.Bio;
            localUser.BirthDate = user.BirthDate;
            localUser.City = user.City;
            localUser.Username = user.Username;

            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                localUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }

            await _context.SaveChangesAsync();

        }
    }
}