using Bokado.Server.Data;
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

        public async Task<List<Models.Challenge>> GetaAllChallenges()
        {
            var challenges = await _context.Challenges.ToListAsync();
            return challenges;
        }
    }
}
