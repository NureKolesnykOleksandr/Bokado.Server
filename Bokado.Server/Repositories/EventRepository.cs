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

        public async Task<Event> CreateEvent(EventDto eventDto, int creatorId)
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
    }
}
