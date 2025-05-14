using Bokado.Server.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        public Task<IdentityResult> BanUser(int userId)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> SelectChallenges(ICollection<int> challengeIds)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UnUser(int userId)
        {
            throw new NotImplementedException();
        }
    }
}
