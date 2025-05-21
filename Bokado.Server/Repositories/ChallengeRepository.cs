using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bokado.Server.Repositories
{
    public class ChallengeRepository : IChallengeRepository
    {
        private readonly SocialNetworkContext _context;

        Dictionary<int, Func<int, bool>> conditions;

        public ChallengeRepository(SocialNetworkContext context)
        {
            _context = context;
            InitializeChallenges();
        }

        private void InitializeChallenges()
        {
            conditions = new Dictionary<int, Func<int, bool>>()
            {
                {
                    1, userId => _context.ChatParticipants
                        .Count(cp => cp.UserId == userId &&
                              DateTime.UtcNow - cp.JoinedAt < TimeSpan.FromDays(7)) >= 3
                },
                {
                    2, userId => _context.Messages
                        .Count(m => m.SenderId == userId &&
                              DateTime.UtcNow - m.SentAt < TimeSpan.FromDays(7)) >= 10
                },
                {
                    3, userId => _context.EventParticipants
                        .Any(ep => ep.UserId == userId &&
                             DateTime.UtcNow - ep.JoinedAt < TimeSpan.FromDays(7))
                },
                {
                    4, userId => _context.Friendships
                        .Count(f => (f.UserId == userId || f.FriendId == userId) &&
                              DateTime.UtcNow - f.CreatedAt < TimeSpan.FromDays(7)) >= 5
                },
                {
                    5, userId => _context.Messages
                        .Where(m => m.SenderId == userId &&
                              DateTime.UtcNow - m.SentAt < TimeSpan.FromDays(7))
                        .Select(m => m.ChatId)
                        .Distinct()
                        .Count() >= 3
                },
                {
                    6, userId => _context.Events
                        .Any(e => e.CreatorId == userId)
                },
                {
                    7, userId => _context.Swipes
                        .Count(s => s.SwipeId == userId &&
                              s.Action == "like") >= 7
                },
                {
                    8, userId => _context.Messages
                        .Any(m => m.SenderId == userId &&
                             DateTime.UtcNow - m.SentAt < TimeSpan.FromDays(7) &&
                             m.Attachment.EndsWith(".mp3"))
                },
                {
                    9, userId => _context.Events
                        .Any(e => e.Participants.Any(p => p.UserId == userId) &&
                             DateTime.UtcNow - e.Date <= TimeSpan.FromDays(7) &&
                             DateTime.UtcNow - e.Date >= TimeSpan.Zero)
                },
                {
                    10, userId =>
                    {
                        var user = _context.Users
                            .Include(u => u.UserInterests)
                            .FirstOrDefault(u => u.UserId == userId);
            
                        return user != null &&
                               user.UserInterests.Any() &&
                               user.BirthDate != null &&
                               user.AvatarUrl != null &&
                               !string.IsNullOrEmpty(user.Bio);
                    }
                }
            };
        }

        public async Task<IdentityResult> CheckChallenge(int challengeId, int userId)
        {
            Challenge challenge = await _context.Challenges.Where(c => c.ChallengeId == challengeId).FirstOrDefaultAsync();

            if (challenge == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "There is no such challenge" });
            }

            if (!challenge.IsActive)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Challenge is not active" });
            }

            UserChallenge userChallenge = await _context.UserChallenges.Where(uc =>uc.IsCompleted && uc.UserId == userId && uc.ChallengeId==challengeId).FirstOrDefaultAsync();

            if (userChallenge != null)
            {
                return IdentityResult.Success;
            }

            try
            {
                if (conditions[challengeId].Invoke(userId)==true)
                {
                    userChallenge = new UserChallenge() 
                    { 
                        ChallengeId =  challengeId , 
                        UserId = userId, 
                        IsCompleted = true, 
                        CompletedAt = DateTime.UtcNow
                    };

                    _context.Add(userChallenge);
                    await _context.Users
                        .Where(u => u.UserId == userId)
                        .ExecuteUpdateAsync(setters => setters
                            .SetProperty(u => u.Level, u => u.Level + challenge.Reward)
                        );

                    await _context.SaveChangesAsync();


                    return IdentityResult.Success;
                }
                return IdentityResult.Failed(new IdentityError { Description = "Challenge failed" });
            }
            catch(Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        public async Task<Event> CreateEvent(EventDto eventDto, int creatorId)
        {
            var newEvent = new Event()
            {
                City = eventDto.City,
                CreatedAt = DateTime.UtcNow,
                CreatorId = creatorId,
                Date = eventDto.Date.ToUniversalTime(),
                Description = eventDto.Description,
                Title = eventDto.Title
            };

            await _context.Events.AddAsync(newEvent);

            await _context.SaveChangesAsync();

            var participant = new EventParticipant()
            {
                EventId = newEvent.EventId,
                UserId = creatorId,
                JoinedAt = DateTime.UtcNow
            };

            await _context.EventParticipants.AddAsync(participant);
            await _context.SaveChangesAsync();

            return newEvent;
        }

        public async Task<List<Challenge>> GetChallenges()
        {
            var challenges = await _context.Challenges.Where(c=>c.IsActive).ToListAsync();
            return challenges;
        }

        public async Task<List<Event>> GetEvents()
        {
            var events = await _context.Events.ToListAsync();
            return events;
        }

        public async Task<IdentityResult> JoinEvent(int eventId, int userId)
        {
            var newParticipant = _context.EventParticipants.Add(new EventParticipant() { EventId = eventId, UserId = userId, JoinedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }
    }
}