using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IAdminRepository
    {
        Task<IdentityResult> BanUser(int userId);
        Task<IdentityResult> UnUser(int userId);
        Task<IdentityResult> SelectChallenges(ICollection<int> challengeIds);

    }
}
