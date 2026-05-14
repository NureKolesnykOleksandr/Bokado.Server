using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Threading;
using Bokado.Server.Services;

namespace Bokado.Server.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SocialNetworkContext _context;
        private readonly FileService _fileService;

        public UserRepository(SocialNetworkContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<UserInfoDto> GetUserProfile(int userId)
        {
            User user = await _context.Users.Where(u=>u.UserId == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new KeyNotFoundException("Цього користувача не було знайдено");
            }

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
                .Include(u => u.ChatParticipants)
                .Include(u => u.EventParticipants)
                .Include(u => u.UserChallenges)
                .Include(u => u.Messages)
                .Include(u => u.CreatedEvents)
                .FirstOrDefaultAsync();

            if(user == null)
            {
                throw new KeyNotFoundException("Цього користувача не було знайдено");
            }

            var userInterestIds = await _context.UserInterests
                .Where(ui => ui.UserId == userId)
                .Select(ui => ui.InterestId)
                .ToListAsync();

            List<Interest> interests = await _context.Interests
                .Where(i => userInterestIds.Contains(i.InterestId))
                .ToListAsync();

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
                ChatParticipants = user.ChatParticipants,
                CreatedEvents = user.CreatedEvents,
                EventParticipants = user.EventParticipants,
                Friends = user.Friends,
                Messages = user.Messages,
                UserChallenges = user.UserChallenges,
                UserInterests = interests
                 
            };
        }
        public async Task UpdateUserProfile(int userId, UpdateUserDto user)
        {
            var localUser = await _context.Users
                .Include(u => u.UserInterests)
                .ThenInclude(ui => ui.Interest)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (localUser == null)
            {
                throw new ArgumentException("User not found");
            }

            if (user.UserIcon != null)
            {
                string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Icons");
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var imageFileName = await _fileService.SaveFileAsync(
                    user.UserIcon,
                    webRootPath,
                    allowedExtensions,
                    Path.GetFileNameWithoutExtension(user.Username));

                if (imageFileName == null)
                {
                    throw new ArgumentException("File wasn't saved");
                }

                localUser.AvatarUrl = $"/Icons/{imageFileName}";
            }

            localUser.Username = user.Username;
            localUser.BirthDate = DateTime.SpecifyKind(user.BirthDate, DateTimeKind.Utc);
            localUser.Bio = user.Bio;
            localUser.Status = user.Status;
            localUser.City = user.City;

            if (!string.IsNullOrEmpty(user.Password))
            {
                localUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }

            if (user.UserInterests != null && user.UserInterests.Any())
            {
                var existingInterests = await _context.Interests.ToListAsync();

                foreach (var interestName in user.UserInterests)
                {
                    if (!existingInterests.Any(i => i.Name.Equals(interestName, StringComparison.OrdinalIgnoreCase)))
                    {
                        var newInterest = new Interest { Name = interestName };
                        _context.Interests.Add(newInterest);
                        await _context.SaveChangesAsync(); 
                        existingInterests.Add(newInterest);
                    }
                }

                var requestedInterestIds = existingInterests
                    .Where(i => user.UserInterests.Contains(i.Name))
                    .Select(i => i.InterestId)
                    .ToList();

                var currentInterestIds = localUser.UserInterests.Select(ui => ui.InterestId).ToList();
                var interestsToAdd = requestedInterestIds.Except(currentInterestIds).ToList();
                var interestsToRemove = currentInterestIds.Except(requestedInterestIds).ToList();

                foreach (var interestId in interestsToAdd)
                {
                    localUser.UserInterests.Add(new UserInterest { InterestId = interestId, UserId = userId });
                }

                foreach (var interestId in interestsToRemove)
                {
                    var ui = localUser.UserInterests.First(ui => ui.InterestId == interestId);
                    _context.UserInterests.Remove(ui);
                }
            }
            else
            {
                _context.UserInterests.RemoveRange(localUser.UserInterests);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUserCount()
        {
            int result = await _context.Users.CountAsync();
            return result;
        }

        public async Task<UserOnlineStatusDto> GetOnlineStatus(int userId)
        {
            var user = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => new { u.UserId, u.LastActive })
                .FirstOrDefaultAsync()
                ?? throw new KeyNotFoundException("Користувача не знайдено");

            return new UserOnlineStatusDto
            {
                UserId = user.UserId,
                IsOnline = DateTime.UtcNow - user.LastActive < TimeSpan.FromMinutes(5),
                LastActive = user.LastActive
            };
        }
    }
}