using Bokado.Server.Data;
using Bokado.Server.Services;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bokado.Server.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly SocialNetworkContext _context;
        private readonly NotificationService _notifications;

        public GroupRepository(SocialNetworkContext context, NotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        public async Task<List<GetGroupDto>> GetGroups()
        {
            var groups = await _context.Groups
                .Include(g => g.Creator)
                .Include(g => g.Members).ThenInclude(m => m.User)
                .Include(g => g.GroupInterests).ThenInclude(gi => gi.Interest)
                .ToListAsync();

            return groups.Select(MapToDto).ToList();
        }

        public async Task<GetGroupDto?> GetGroup(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.Creator)
                .Include(g => g.Members).ThenInclude(m => m.User)
                .Include(g => g.GroupInterests).ThenInclude(gi => gi.Interest)
                .FirstOrDefaultAsync(g => g.GroupId == groupId);

            return group == null ? null : MapToDto(group);
        }

        public async Task<GetGroupDto> CreateGroup(CreateGroupDto dto, int creatorId)
        {
            var creator = await _context.Users.FindAsync(creatorId)
                ?? throw new KeyNotFoundException("User not found");

            if (!creator.IsPremium)
            {
                var createdCount = await _context.Groups.CountAsync(g => g.CreatorId == creatorId);
                if (createdCount >= 3)
                    throw new InvalidOperationException("Звичайні користувачі можуть створити не більше 3 груп. Оформіть Premium для необмеженої кількості груп.");
            }

            var chat = new Chat { CreatedAt = DateTime.UtcNow };
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            var group = new Group
            {
                Name = dto.Name,
                Description = dto.Description,
                City = dto.City,
                Status = GroupStatus.Open,
                CreatorId = creatorId,
                ChatId = chat.ChatId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            _context.GroupMembers.Add(new GroupMember
            {
                GroupId = group.GroupId,
                UserId = creatorId,
                JoinedAt = DateTime.UtcNow,
                Role = GroupMemberRole.Owner
            });

            _context.ChatParticipants.Add(new ChatParticipant
            {
                ChatId = chat.ChatId,
                UserId = creatorId,
                JoinedAt = DateTime.UtcNow
            });

            if (dto.InterestIds.Any())
            {
                var validIds = await _context.Interests
                    .Where(i => dto.InterestIds.Contains(i.InterestId))
                    .Select(i => i.InterestId)
                    .ToListAsync();

                foreach (var interestId in validIds)
                {
                    _context.GroupInterests.Add(new GroupInterest
                    {
                        GroupId = group.GroupId,
                        InterestId = interestId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return (await GetGroup(group.GroupId))!;
        }

        public async Task<IdentityResult> UpdateGroup(int groupId, int userId, UpdateGroupDto dto)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                return IdentityResult.Failed(new IdentityError { Description = "Групу не знайдено" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Користувача не знайдено" });

            if (group.CreatorId != userId && !user.IsAdmin)
                return IdentityResult.Failed(new IdentityError { Description = "Немає прав для редагування цієї групи" });

            if (group.Status == GroupStatus.Closed)
                return IdentityResult.Failed(new IdentityError { Description = "Закриту групу не можна редагувати" });

            group.Name = dto.Name ?? group.Name;
            group.Description = dto.Description ?? group.Description;
            group.City = dto.City ?? group.City;

            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteGroup(int groupId, int userId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                return IdentityResult.Failed(new IdentityError { Description = "Групу не знайдено" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Користувача не знайдено" });

            if (group.CreatorId != userId && !user.IsAdmin)
                return IdentityResult.Failed(new IdentityError { Description = "Немає прав для видалення цієї групи" });

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> CloseGroup(int groupId, int userId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                return IdentityResult.Failed(new IdentityError { Description = "Групу не знайдено" });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "Користувача не знайдено" });

            if (group.CreatorId != userId && !user.IsAdmin)
                return IdentityResult.Failed(new IdentityError { Description = "Тільки власник може закрити групу" });

            if (group.Status == GroupStatus.Closed)
                return IdentityResult.Failed(new IdentityError { Description = "Група вже закрита" });

            group.Status = GroupStatus.Closed;
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> JoinGroup(int groupId, int userId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                return IdentityResult.Failed(new IdentityError { Description = "Групу не знайдено" });

            if (group.Status == GroupStatus.Closed)
                return IdentityResult.Failed(new IdentityError { Description = "Група закрита для вступу" });

            if (await _context.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId))
                return IdentityResult.Failed(new IdentityError { Description = "Ви вже є учасником цієї групи" });

            var creator = await _context.Users.FindAsync(group.CreatorId);
            int memberLimit = (creator != null && creator.IsPremium) ? 100 : 10;
            int currentCount = await _context.GroupMembers.CountAsync(m => m.GroupId == groupId);
            if (currentCount >= memberLimit)
            {
                string hint = (creator != null && !creator.IsPremium)
                    ? " Власник групи може збільшити ліміт до 100, оформивши Premium."
                    : "";
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"Група заповнена (максимум {memberLimit} учасників).{hint}"
                });
            }

            _context.GroupMembers.Add(new GroupMember
            {
                GroupId = groupId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                Role = GroupMemberRole.Member
            });

            _context.ChatParticipants.Add(new ChatParticipant
            {
                ChatId = group.ChatId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            });

            var joiner = await _context.Users.FindAsync(userId);
            _context.Messages.Add(new Message
            {
                ChatId = group.ChatId,
                SenderId = userId,
                Text = $"🙋 {joiner?.Username ?? "Користувач"} приєднався до групи",
                SentAt = DateTime.UtcNow,
                Attachment = ""
            });

            await _context.SaveChangesAsync();

            // 🔔 Сповіщення власнику групи
            if (group.CreatorId != userId)
            {
                var joiner = await _context.Users.FindAsync(userId);
                if (joiner != null)
                    await _notifications.GroupJoinedAsync(group.CreatorId, userId, joiner.Username, groupId, group.Name);
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> LeaveGroup(int groupId, int userId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                return IdentityResult.Failed(new IdentityError { Description = "Групу не знайдено" });

            if (group.CreatorId == userId)
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
                return IdentityResult.Success;
            }

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == userId);

            if (member == null)
                return IdentityResult.Failed(new IdentityError { Description = "Ви не є учасником цієї групи" });

            _context.GroupMembers.Remove(member);

            var chatParticipant = await _context.ChatParticipants
                .FirstOrDefaultAsync(cp => cp.ChatId == group.ChatId && cp.UserId == userId);

            if (chatParticipant != null)
                _context.ChatParticipants.Remove(chatParticipant);

            var leaver = await _context.Users.FindAsync(userId);
            _context.Messages.Add(new Message
            {
                ChatId = group.ChatId,
                SenderId = userId,
                Text = $"👋 {leaver?.Username ?? "Користувач"} покинув групу",
                SentAt = DateTime.UtcNow,
                Attachment = ""
            });

            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> KickMember(int groupId, int requesterId, int targetUserId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                return IdentityResult.Failed(new IdentityError { Description = "Групу не знайдено" });

            var requester = await _context.Users.FindAsync(requesterId);
            if (requester == null)
                return IdentityResult.Failed(new IdentityError { Description = "Користувача не знайдено" });

            if (group.CreatorId != requesterId && !requester.IsAdmin)
                return IdentityResult.Failed(new IdentityError { Description = "Тільки власник або адмін може видаляти учасників" });

            if (group.CreatorId == targetUserId)
                return IdentityResult.Failed(new IdentityError { Description = "Не можна видалити власника групи" });

            var member = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == targetUserId);

            if (member == null)
                return IdentityResult.Failed(new IdentityError { Description = "Цей користувач не є учасником групи" });

            _context.GroupMembers.Remove(member);

            var chatParticipant = await _context.ChatParticipants
                .FirstOrDefaultAsync(cp => cp.ChatId == group.ChatId && cp.UserId == targetUserId);

            if (chatParticipant != null)
                _context.ChatParticipants.Remove(chatParticipant);

            var kicked = await _context.Users.FindAsync(targetUserId);
            _context.Messages.Add(new Message
            {
                ChatId = group.ChatId,
                SenderId = requesterId,
                Text = $"🚫 {kicked?.Username ?? "Користувач"} був видалений з групи",
                SentAt = DateTime.UtcNow,
                Attachment = ""
            });

            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AssignAdmin(int groupId, int requesterId, int targetUserId)
        {
            var group = await _context.Groups.FindAsync(groupId);
            if (group == null)
                return IdentityResult.Failed(new IdentityError { Description = "Групу не знайдено" });

            var requesterMember = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == requesterId);

            if (requesterMember == null || requesterMember.Role != GroupMemberRole.Owner)
                return IdentityResult.Failed(new IdentityError { Description = "Тільки власник може призначати адміністраторів" });

            var targetMember = await _context.GroupMembers
                .FirstOrDefaultAsync(m => m.GroupId == groupId && m.UserId == targetUserId);

            if (targetMember == null)
                return IdentityResult.Failed(new IdentityError { Description = "Користувач не є учасником групи" });

            if (targetMember.Role == GroupMemberRole.Owner)
                return IdentityResult.Failed(new IdentityError { Description = "Не можна змінити роль власника" });

            targetMember.Role = targetMember.Role == GroupMemberRole.Admin
                ? GroupMemberRole.Member
                : GroupMemberRole.Admin;

            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<List<GetGroupDto>> GetRecommendations(int userId)
        {
            var random = new Random();

            var currentUser = await _context.Users
                .Where(u => u.UserId == userId)
                .Include(u => u.UserInterests)
                .FirstOrDefaultAsync();

            var userInterestIds = currentUser?.UserInterests?
                .Select(ui => ui.InterestId).ToHashSet() ?? new HashSet<int>();

            var joinedGroupIds = await _context.GroupMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.GroupId)
                .ToListAsync();

            var groups = await _context.Groups
                .Where(g => g.Status == GroupStatus.Open && !joinedGroupIds.Contains(g.GroupId))
                .Include(g => g.Creator)
                .Include(g => g.Members).ThenInclude(m => m.User)
                .Include(g => g.GroupInterests).ThenInclude(gi => gi.Interest)
                .ToListAsync();

            var scored = groups.Select(g =>
            {
                double score = 0;

                // City match (0.3)
                if (!string.IsNullOrEmpty(currentUser?.City) && g.City == currentUser.City)
                    score += 0.3;

                // Shared interests (0.4)
                if (userInterestIds.Any())
                {
                    int common = g.GroupInterests.Count(gi => userInterestIds.Contains(gi.InterestId));
                    int maxPossible = Math.Min(g.GroupInterests.Count, userInterestIds.Count);
                    if (maxPossible > 0) score += 0.4 * ((double)common / maxPossible);
                }

                // Freshness — group age (0.2)
                double ageDays = (DateTime.UtcNow - g.CreatedAt).TotalDays;
                score += 0.2 * Math.Max(0, 1 - ageDays / 90);

                // Popularity — member count (0.1)
                score += 0.1 * Math.Min(1, g.Members.Count / 10.0);

                return new { Group = g, Weight = Math.Max(score, 0.01) };
            }).ToList();

            // Weighted random selection (mirrors friend search algorithm)
            var result = new List<GetGroupDto>();
            for (int i = 0; i < 3 && scored.Any(); i++)
            {
                double total = scored.Sum(w => w.Weight);
                double rand = random.NextDouble() * total;
                double cumulative = 0;
                foreach (var sg in scored)
                {
                    cumulative += sg.Weight;
                    if (cumulative >= rand)
                    {
                        result.Add(MapToDto(sg.Group));
                        scored.Remove(sg);
                        break;
                    }
                }
            }

            return result;
        }

        public async Task<string> StartGroupCall(int groupId, int userId)
        {
            var group = await _context.Groups.FindAsync(groupId)
                ?? throw new KeyNotFoundException("Групу не знайдено");

            if (group.Status == GroupStatus.Closed)
                throw new InvalidOperationException("Група закрита");

            if (!await _context.GroupMembers.AnyAsync(m => m.GroupId == groupId && m.UserId == userId))
                throw new UnauthorizedAccessException("Ви не є учасником цієї групи");

            var meetLink = GenerateMeetLink();

            var message = new Message
            {
                ChatId = group.ChatId,
                SenderId = userId,
                Text = $"Відеодзвінок розпочато: {meetLink}",
                SentAt = DateTime.UtcNow,
                Attachment = ""
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return meetLink;
        }

        private static string GenerateMeetLink()
        {
            var roomId = Guid.NewGuid().ToString("N")[..12];
            return $"https://meet.jit.si/Bokado-{roomId}";
        }

        private static GetGroupDto MapToDto(Group group) => new()
        {
            GroupId = group.GroupId,
            Name = group.Name,
            Description = group.Description,
            City = group.City,
            Status = group.Status,
            CreatorId = group.CreatorId,
            ChatId = group.ChatId,
            CreatedAt = group.CreatedAt,
            MaxMembers = group.Creator?.IsPremium == true ? 100 : 10,
            Creator = new UserDto
            {
                UserId = group.Creator.UserId,
                Username = group.Creator.Username,
                Email = group.Creator.Email,
                IsAdmin = group.Creator.IsAdmin
            },
            Members = group.Members.Select(m => new GroupMemberDto
            {
                UserId = m.User.UserId,
                Username = m.User.Username,
                AvatarUrl = m.User.AvatarUrl,
                Role = m.Role
            }).ToList(),
            Interests = group.GroupInterests.Select(gi => new GroupInterestDto
            {
                InterestId = gi.Interest.InterestId,
                Name = gi.Interest.Name
            }).ToList()
        };
    }
}
