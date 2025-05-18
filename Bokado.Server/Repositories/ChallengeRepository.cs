using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Repositories
{
    public class ChallengeRepository : IChallengeRepository
    {
        public Task<IdentityResult> CheckChallenge(int challengeId)
        {
            throw new NotImplementedException();
        }

        public Task<Event> CreateEvent(EventDto eventDto, int creatorId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Challenge>> GetChallenges()
        {
            throw new NotImplementedException();
        }

        public Task<List<Event>> GetEvents()
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> JoinEvent(int eventId, int userId)
        {
            throw new NotImplementedException();
        }
    }
}
