using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Cmp;

namespace Bokado.Server.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly SocialNetworkContext _context;

        public AdminRepository(SocialNetworkContext context)
        {
            _context = context;
        }

        public async Task<IdentityResult> BanUser(int userId)
        {
            User user = await _context.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "There is no such user" });
            }

            user.IsBanned = true;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SelectChallenges(ICollection<int> challengeIds)
        {
            try
            {
                _context.UserChallenges.ExecuteDelete();
                await _context.Challenges
                    .Where(c => c.IsActive)
                    .ExecuteUpdateAsync(setters => setters
                     .SetProperty(c => c.IsActive, false)
                            );

                await _context.Challenges
                    .Where(c => challengeIds.Contains(c.ChallengeId))
                    .ExecuteUpdateAsync(setterts => setterts
                    .SetProperty(c => c.IsActive, true));

                await _context.SaveChangesAsync();

                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<IdentityResult> UnbanUser(int userId)
        {
            User user = await _context.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "There is no such user" });
            }

            user.IsBanned = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<IEnumerable<Models.Challenge>> GetaAllChallenges()
        {
            var challenges = await _context.Challenges.ToListAsync();
            return challenges;
        }

        public async Task<IEnumerable<UserDetailInfoDto>> GetaAllUsers()
        {

            List<User> users = await _context.Users
                .Include(u => u.UserInterests)
                .Include(u => u.Friends)
                .Include(u => u.Swipes)
                .Include(u => u.ChatParticipants)
                .Include(u => u.EventParticipants)
                .Include(u => u.UserChallenges)
                .Include(u => u.Messages)
                .Include(u => u.CreatedEvents)
                .ToListAsync();

            return users.Select(user => new UserDetailInfoDto()
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
                UserId = user.UserId,
                Username = user.Username,
                Swipes = user.Swipes,
                ChatParticipants = user.ChatParticipants,
                CreatedEvents = user.CreatedEvents,
                EventParticipants = user.EventParticipants,
                Friends = user.Friends,
                Messages = user.Messages,
                UserChallenges = user.UserChallenges,
                UserInterests = user.UserInterests
            });
        } 
    }
}
