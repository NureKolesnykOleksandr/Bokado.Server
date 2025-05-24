using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Threading;

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

        public async Task UpdateUserProfile(int userId, UpdateUserDto user)
        {
            var localUser = await _context.Users
                .Include(u => u.UserInterests)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (localUser == null)
            {
                throw new ArgumentException("User not found");
            }

            if (user.UserIcon != null)
            {
                string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Icons");
                Directory.CreateDirectory(webRootPath);

                // Получаем расширение файла
                var fileExtension = Path.GetExtension(user.UserIcon.FileName).ToLower();

                // Генерируем имя файла с сохранением расширения
                var imageFileName = $"{DateTime.UtcNow.Ticks}_{Path.GetFileNameWithoutExtension(user.Username)}{fileExtension}";
                var imageDestinationPath = Path.Combine(webRootPath, imageFileName);

                // Проверяем допустимые расширения (опционально)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new ArgumentException("Invalid file format. Allowed formats: " + string.Join(", ", allowedExtensions));
                }

                using (var stream = new FileStream(imageDestinationPath, FileMode.Create))
                {
                    await user.UserIcon.CopyToAsync(stream);
                }

                localUser.AvatarUrl = $"/Icons/{imageFileName}";
            }

            // Обновляем основные данные пользователя
            localUser.Username = user.Username;
            localUser.BirthDate = user.BirthDate;
            localUser.Bio = user.Bio;
            localUser.Status = user.Status;
            localUser.City = user.City;

            if (!string.IsNullOrEmpty(user.Password))
            {
                localUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }

            // Обновляем интересы пользователя
            if (user.UserInterestsIds != null && user.UserInterestsIds.Any())
            {
                var allInterests = await _context.Interests
                    .Where(i => user.UserInterestsIds.Contains(i.InterestId))
                    .ToListAsync();

                if (allInterests.Count != user.UserInterestsIds.Distinct().Count())
                {
                    throw new KeyNotFoundException("One or more interests were not found");
                }

                var currentInterestIds = localUser.UserInterests.Select(ui => ui.InterestId).ToList();
                var interestsToAdd = user.UserInterestsIds.Except(currentInterestIds).ToList();
                var interestsToRemove = currentInterestIds.Except(user.UserInterestsIds).ToList();

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
    }
}