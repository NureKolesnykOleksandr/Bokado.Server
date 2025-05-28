using Bokado.Server.Dtos;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IChallengeRepository
    {
        Task<List<ChallengeDto>> GetChallenges(int userId);
        Task<IdentityResult> CheckChallenge(int challengeId, int userId);
        Task<List<Event>> GetEvents();
        Task<Event> CreateEvent(EventDto eventDto, int creatorId);
        Task<IdentityResult> JoinEvent(int eventId, int userId);
        Task<IdentityResult> QuitEvent(int eventId, int userId);
    }
}
