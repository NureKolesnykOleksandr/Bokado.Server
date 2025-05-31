using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Bokado.Server.Repositories
{
    public class StatisticRepository : IStatisticRepository
    {
        private readonly SocialNetworkContext _context;

        public StatisticRepository(SocialNetworkContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, int>> GetChallengesCompleted()
        {
            var activeChallenges = await _context.Challenges.Where(c => c.IsActive).ToListAsync();

            Dictionary<string, int> result = new();

            foreach (var challenge in activeChallenges) 
            {
                int number = await _context.UserChallenges.Where(uc => uc.IsCompleted && uc.ChallengeId == challenge.ChallengeId).CountAsync();
                result[challenge.Title] = number;
            }
            return result;
        }

        public async Task<List<Dictionary<DateOnly, List<UserInfoDto>>>> GetUsersPerMonth()
        {
            var users = await _context.Users
                .OrderBy(u => u.CreatedAt)
                .Select(u => new UserInfoDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    BirthDate = u.BirthDate,
                    AvatarUrl = u.AvatarUrl,
                    Bio = u.Bio,
                    Status = u.Status,
                    Level = u.Level,
                    City = u.City,
                    IsPremium = u.IsPremium,
                    IsBanned = u.IsBanned,
                    IsAdmin = u.IsAdmin,
                    CreatedAt = u.CreatedAt,
                    LastActive = u.LastActive
                })
                .ToListAsync();

            var result = users
                .GroupBy(u => new DateOnly(u.CreatedAt.Year, u.CreatedAt.Month, 1))
                .OrderBy(g => g.Key)
                .Select(g => new Dictionary<DateOnly, List<UserInfoDto>>
                {
                    { g.Key, g.ToList() }
                })
                .ToList();

            return result;
        }
    }
}
