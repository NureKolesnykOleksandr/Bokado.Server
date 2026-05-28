using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Bokado.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Bokado.Server.Repositories
{
    public class EventRepository : IEventRepository
    {
        SocialNetworkContext _context;
        private readonly NotificationService _notifications;

        public EventRepository(SocialNetworkContext context, NotificationService notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        public async Task<Event> CreateEvent(CreateEventDto eventDto, int creatorId)
        {
            if (eventDto.Maximum < 2)
                throw new ValidationException("You can`t create event with less then 2 participants");

            var newEvent = new Event()
            {
                City = eventDto.City,
                CreatedAt = DateTime.UtcNow,
                CreatorId = creatorId,
                Date = eventDto.Date.ToUniversalTime(),
                Description = eventDto.Description,
                Title = eventDto.Title,
                Maximum = eventDto.Maximum
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

        public async Task<List<GetEventDto>> GetEvents()
        {
            var events = await _context.Events
                .Include(e => e.Creator)
                .ToListAsync();

            var eventParticipants = await _context.EventParticipants
                .Where(ep => events.Select(e => e.EventId).Contains(ep.EventId))
                .Include(ep => ep.User)
                .ToListAsync();

            var participantsByEventId = eventParticipants
                .GroupBy(ep => ep.EventId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ep => new UserDto
                    {
                        UserId = ep.User.UserId,
                        Username = ep.User.Username,
                        Email = ep.User.Email,
                        IsAdmin = ep.User.IsAdmin
                    }).ToList()
                );

            var result = events.Select(e => new GetEventDto
            {
                EventId = e.EventId,
                Title = e.Title,
                Description = e.Description,
                Date = e.Date,
                City = e.City,
                Maximum = e.Maximum,
                CreatorId = e.CreatorId,
                CreatedAt = e.CreatedAt,
                Creator = new UserDto
                {
                    UserId = e.Creator.UserId,
                    Username = e.Creator.Username,
                    Email = e.Creator.Email,
                    IsAdmin = e.Creator.IsAdmin
                },
                Participants = participantsByEventId.TryGetValue(e.EventId, out var participants)
                    ? participants
                    : new List<UserDto>()
            }).ToList();

            return result;
        }

        public async Task<IdentityResult> JoinEvent(int eventId, int userId)
        {
            var ev = await _context.Events.Where(e => e.EventId == eventId).FirstOrDefaultAsync();
            if (ev == null)
                return IdentityResult.Failed(new IdentityError { Description = "Цього івенту не існує" });

            if (await _context.EventParticipants.AnyAsync(ep => ep.EventId == eventId && ep.UserId == userId))
                return IdentityResult.Failed(new IdentityError { Description = "Ви вже приймаєте участь у цьому івенті" });

            if (await _context.EventParticipants.Where(ep => ep.EventId == eventId).CountAsync() >= ev.Maximum)
                return IdentityResult.Failed(new IdentityError { Description = "Максимум людей вже була досягнута" });

            _context.EventParticipants.Add(new EventParticipant
            {
                EventId = eventId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // 🔔 Сповіщення організатору (якщо не сам організатор)
            if (ev.CreatorId != userId)
            {
                var joiner = await _context.Users.FindAsync(userId);
                if (joiner != null)
                    await _notifications.EventJoinedAsync(ev.CreatorId, userId, joiner.Username, eventId, ev.Title);
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> QuitEvent(int eventId, int userId)
        {
            var user = await _context.EventParticipants
                .Where(ep => ep.UserId == userId && ep.EventId == eventId)
                .FirstOrDefaultAsync();

            if (user == null)
                return IdentityResult.Failed(new IdentityError() { Description = "Вашої участі у цьому івенту не було знайдено" });

            _context.EventParticipants.Remove(user);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateEvent(int eventId, int userId, UpdateEventDto eventDto)
        {
            var ev = await _context.Events.FindAsync(eventId);
            var user = await _context.Users.FindAsync(userId);

            if (ev == null)
                return IdentityResult.Failed(new IdentityError { Description = "Event was not found" });
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User was not found" });
            if (ev.CreatorId != userId && !user.IsAdmin)
                return IdentityResult.Failed(new IdentityError { Description = "You don`t have permission to update this Event" });

            ev.Title = eventDto.Title ?? ev.Title;
            ev.Description = eventDto.Description ?? ev.Description;
            ev.Date = eventDto.Date ?? ev.Date;
            ev.City = eventDto.City ?? ev.City;

            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }
    }
}
