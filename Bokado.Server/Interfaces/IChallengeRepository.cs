using Bokado.Server.Dtos;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IChallengeRepository
    {
        Task<List<ChallengeDto>> GetChallenges(int userId);
        Task<IdentityResult> CheckChallenge(int challengeId, int userId);
    }
}
