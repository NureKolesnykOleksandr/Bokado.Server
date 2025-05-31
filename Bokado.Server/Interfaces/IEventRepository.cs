using Bokado.Server.Dtos;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IEventRepository
    {
        Task<List<GetEventDto>> GetEvents();
        Task<Event> CreateEvent(CreateEventDto eventDto, int creatorId);
        Task<IdentityResult> JoinEvent(int eventId, int userId);
        Task<IdentityResult> QuitEvent(int eventId, int userId);
    }
}
