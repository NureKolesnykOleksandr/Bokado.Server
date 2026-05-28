using Bokado.Server.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;

namespace Bokado.Server.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private static readonly ConcurrentDictionary<int, HashSet<string>> _onlineUsers = new();
        private readonly SocialNetworkContext _context;

        public ChatHub(SocialNetworkContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            if (userId <= 0) return;

            _onlineUsers.AddOrUpdate(
                userId,
                _ => new HashSet<string> { Context.ConnectionId },
                (_, set) => { lock (set) { set.Add(Context.ConnectionId); } return set; }
            );

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastActive = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            await NotifyContactsAboutStatus(userId, isOnline: true);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            if (userId <= 0) return;

            bool wentOffline = false;

            if (_onlineUsers.TryGetValue(userId, out var set))
            {
                lock (set) { set.Remove(Context.ConnectionId); }
                if (set.Count == 0)
                {
                    _onlineUsers.TryRemove(userId, out _);
                    wentOffline = true;

                    var user = await _context.Users.FindAsync(userId);
                    if (user != null)
                    {
                        user.LastActive = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            if (wentOffline)
                await NotifyContactsAboutStatus(userId, isOnline: false);

            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendTyping(int chatId, bool isTyping)
        {
            var senderId = GetUserId();
            if (senderId <= 0) return;

            var participantIds = await _context.ChatParticipants
                .Where(cp => cp.ChatId == chatId && cp.UserId != senderId)
                .Select(cp => cp.UserId)
                .ToListAsync();

            foreach (var recipientId in participantIds)
            {
                if (_onlineUsers.TryGetValue(recipientId, out var connections))
                {
                    foreach (var connId in connections.ToArray())
                        await Clients.Client(connId).SendAsync("ReceiveTyping", chatId, senderId, isTyping);
                }
            }
        }

        public static bool IsOnline(int userId) => _onlineUsers.ContainsKey(userId);

        private async Task NotifyContactsAboutStatus(int userId, bool isOnline)
        {
            var contactIds = await _context.ChatParticipants
                .Where(cp => cp.UserId == userId)
                .SelectMany(cp => _context.ChatParticipants
                    .Where(other => other.ChatId == cp.ChatId && other.UserId != userId)
                    .Select(other => other.UserId))
                .Distinct()
                .ToListAsync();

            foreach (var contactId in contactIds)
            {
                if (_onlineUsers.TryGetValue(contactId, out var connections))
                {
                    foreach (var connId in connections.ToArray())
                        await Clients.Client(connId).SendAsync("UserOnlineStatus", userId, isOnline);
                }
            }
        }

        private int GetUserId()
        {
            try
            {
                var token = Context.GetHttpContext()?.Request.Query["access_token"].ToString();
                if (string.IsNullOrEmpty(token)) return 0;
                var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid");
                return claim != null && int.TryParse(claim.Value, out int id) ? id : 0;
            }
            catch { return 0; }
        }
    }
}
