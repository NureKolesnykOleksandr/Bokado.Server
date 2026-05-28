using Bokado.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Bokado.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly SocialNetworkContext _db;

        public NotificationController(SocialNetworkContext db)
        {
            _db = db;
        }

        // GET api/notification — останні 30 сповіщень
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            var items = await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(30)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Type,
                    n.Message,
                    n.Link,
                    n.IsRead,
                    n.CreatedAt,
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET api/notification/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var userId = GetUserId();
            var count = await _db.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
            return Ok(new { count });
        }

        // PATCH api/notification/{id}/read
        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = GetUserId();
            var n = await _db.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);
            if (n == null) return NotFound();

            n.IsRead = true;
            await _db.SaveChangesAsync();
            return Ok();
        }

        // PATCH api/notification/read-all
        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetUserId();
            await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
            return Ok();
        }

        private int GetUserId()
        {
            var token = HttpContext.Request.Headers["Authorization"]
                .FirstOrDefault()?.Split(" ").Last();
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var claim = jwt.Claims.FirstOrDefault(c => c.Type == "nameid");
            if (claim == null || !int.TryParse(claim.Value, out int id))
                throw new SecurityTokenException("User ID not found");
            return id;
        }
    }
}
