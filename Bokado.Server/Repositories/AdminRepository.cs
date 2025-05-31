using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Cmp;
using System.Collections.Generic;

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

            var allInterestIds = users.SelectMany(u => u.UserInterests.Select(ui => ui.InterestId)).Distinct();

            var allInterests = await _context.Interests
                .Where(i => allInterestIds.Contains(i.InterestId))
                .ToListAsync();

            return users.Select(user => new UserDetailInfoDto()
            {
                UserId = user.UserId,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl,
                Bio= user.Bio,
                BirthDate = user.BirthDate,
                City = user.City,
                Email = user.Email,
                CreatedAt  = user.CreatedAt,
                IsAdmin = user.IsAdmin,
                LastActive = user.LastActive,
                IsBanned = user.IsBanned,
                IsPremium = user.IsPremium,
                Level = user.Level,
                Status = user.Status,
                Swipes = user.Swipes.Select(s=> new Swipe { Action = s.Action, IsMatch = s.IsMatch, SwipedAt = s.SwipedAt, SwipeId = s.SwipeId, TargetUser = s.TargetUser, TargetUserId = s.TargetUserId, SwiperId = s.SwiperId}).ToList(),
                Messages = user.Messages.Select(m=> new Message { MessageId = m.MessageId, Attachment = m.Attachment, Text = m.Text, ChatId = m.ChatId, IsRead = m.IsRead, SenderId = m.SenderId, SentAt = m.SentAt}).ToList(),
                Friends = user.Friends.Select(f=> new Friendship { CreatedAt = f.CreatedAt, FriendId = f.FriendId, FriendshipId = f.FriendshipId, UserId = f.UserId}).ToList(),
                UserChallenges = user.UserChallenges.Select(uc=> new UserChallenge { Challenge = uc.Challenge, UserId = uc.UserId, ChallengeId = uc.ChallengeId, CompletedAt = uc.CompletedAt, IsCompleted = uc.IsCompleted, UserChallengeId = uc.UserChallengeId}).ToList(),
                CreatedEvents = user.CreatedEvents.Select(ce=> new Event { City = ce.City, CreatedAt = ce.CreatedAt, CreatorId = ce.CreatorId, Date = ce.Date, Description = ce.Description, EventId = ce.EventId, Maximum = ce.Maximum, Title = ce.Title}).ToList(),
                EventParticipants = user.EventParticipants.Select(ep=> new EventParticipant { Event = ep.Event, EventId = ep.EventId, EventParticipantId = ep.EventParticipantId, JoinedAt = ep.JoinedAt, UserId = ep.UserId}).ToList(),
                ChatParticipants = user.ChatParticipants.Select(cp=> new ChatParticipant { UserId = cp.UserId, JoinedAt = cp.JoinedAt, ChatId = cp.ChatId, Chat = cp.Chat, ChatParticipantId = cp.ChatParticipantId}).ToList(),
                UserInterests = allInterests
                    .Where(i => user.UserInterests.Any(ui => ui.InterestId == i.InterestId))
                    .ToList()
            }).ToList();
        } 
    }
}
