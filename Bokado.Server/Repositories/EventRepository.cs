using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Bokado.Server.Repositories
{
    public class EventRepository : IEventRepository
    {
        SocialNetworkContext _context;

        public EventRepository(SocialNetworkContext context)
        {
            _context = context;
        }

        public async Task<Event> CreateEvent(CreateEventDto eventDto, int creatorId)
        {
            if (eventDto.Maximum < 2)
            {
                throw new ValidationException("You can`t create event with less then 2 participants");
            }

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
            // Get all events
            var events = await _context.Events
                .Include(e => e.Creator)
                .ToListAsync();

            // Get all participants for these events
            var eventParticipants = await _context.EventParticipants
                .Where(ep => events.Select(e => e.EventId).Contains(ep.EventId))
                .Include(ep => ep.User)
                .ToListAsync();

            // Create a lookup for participants by event ID
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
            var Event = await _context.Events.Where(e => e.EventId == eventId).FirstOrDefaultAsync();
            if (Event == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Цього івенту не існує" });
            }

            if (await _context.EventParticipants.AnyAsync(ep => ep.EventId == eventId && ep.UserId == userId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Ви вже приймаєте участь у цьому івенті"});
            }


            if (await _context.EventParticipants.Where(ep => ep.EventId == eventId).CountAsync() >= Event.Maximum)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Максимум людей вже була досягнута" });
            }
            
            var newParticipant = _context.EventParticipants.Add(new EventParticipant() { EventId = eventId, UserId = userId, JoinedAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> QuitEvent(int eventId, int userId)
        {
            var user = await _context.EventParticipants.Where(ep => ep.UserId == userId && ep.EventId == eventId).FirstOrDefaultAsync();

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError() { Description = "Вашої участі у цьому івенту не було знайдено" });
            }

            _context.EventParticipants.Remove(user);
            await _context.SaveChangesAsync();

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateEvent(int eventId, int userId, UpdateEventDto eventDto)
        {
            var Event = await _context.Events.FindAsync(eventId);
            var user = await _context.Users.FindAsync(userId);

            if (Event == null)
            {
                return IdentityResult.Failed(new IdentityError {  Description = "Event was not found"});
            }
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User was not found" });
            }
            if (Event.CreatorId != userId && !user.IsAdmin)
            {
                return IdentityResult.Failed(new IdentityError { Description = "You don`t have permission to update this Event" });
            }

            Event.Title = eventDto.Title ?? Event.Title;
            Event.Description = eventDto.Description ?? Event.Description;
            Event.Date = eventDto.Date ?? Event.Date;
            Event.City = eventDto.City ?? Event.City;

            await _context.SaveChangesAsync();

            return IdentityResult.Success;

        }
    }
}
