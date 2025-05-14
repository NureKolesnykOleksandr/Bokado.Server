using Bokado.Server.Dtos;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IChallengeRepository
    {
        Task<List<Challenge>> GetChallenges();
        Task<IdentityResult> CheckChallenge(int challengeId);
        Task<List<Event>> GetEvents();
        Task<Event> CreateEvent(EventDto eventDto, int creatorId);
    }
}
