using Bokado.Server.Data;
using Bokado.Server.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bokado.Server.Repositories
{
    public class SubscribeRepository : ISubscribeRepository
    {
        private readonly SocialNetworkContext _context;

        public SubscribeRepository(SocialNetworkContext context)
        {
            _context = context;
        }

        public async Task<IdentityResult> GiveSubscribe(int userId)
        {
            var user = await _context.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();
            if(user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Цього юзера не було знайдено" });
            }

            if (user.IsPremium)
            {
                return IdentityResult.Failed(new IdentityError { Description = "В цього юзера є підписка" });
            }
            if (user.IsBanned) user.IsBanned = false;
            user.IsPremium = true;
            
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> TakeSubscribe(int userId)
        {
            var user = await _context.Users.Where(u => u.UserId == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Цього юзера не було знайдено" });
            }

            if (!user.IsPremium)
            {
                return IdentityResult.Failed(new IdentityError { Description = "В цього юзера немає підписки" });
            }
            user.IsPremium = false;
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
    }
}
