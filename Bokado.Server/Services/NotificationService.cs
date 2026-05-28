// Services/NotificationService.cs
using Bokado.Server.Data;
using Bokado.Server.Hubs;
using Bokado.Server.Models;
using Microsoft.AspNetCore.SignalR;

namespace Bokado.Server.Services
{
    public class NotificationService
    {
        private readonly SocialNetworkContext _db;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly EmailService _email;

        public NotificationService(
            SocialNetworkContext db,
            IHubContext<NotificationHub> hub,
            EmailService email)
        {
            _db = db;
            _hub = hub;
            _email = email;
        }

        public async Task SendAsync(
            int toUserId,
            int? actorId,
            NotificationType type,
            string message,
            string? link = null)
        {
            // 1. Зберігаємо в БД — чекаємо
            var notification = new Notification
            {
                UserId    = toUserId,
                ActorId   = actorId,
                Type      = type,
                Message   = message,
                Link      = link,
                IsRead    = false,
                CreatedAt = DateTime.UtcNow,
            };
            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();

            // 2. SignalR і Email — у фоні, без await
            // HTTP запит не чекає на SignalR — вирішує проблему зависання на Railway
            _ = Task.Run(async () =>
            {
                try
                {
                    var payload = new
                    {
                        notification.NotificationId,
                        notification.Type,
                        notification.Message,
                        notification.Link,
                        notification.CreatedAt,
                        IsRead = false,
                    };
                    await _hub.Clients
                        .Group($"user_{toUserId}")
                        .SendAsync("ReceiveNotification", payload);
                }
                catch { /* не блокуємо */ }

                try { await TrySendEmailAsync(toUserId, message); }
                catch { /* не блокуємо */ }
            });
        }

        public Task FriendRequestAsync(int toUserId, int fromUserId, string fromUsername)
            => SendAsync(toUserId, fromUserId, NotificationType.FriendRequest,
                $"{fromUsername} надіслав(ла) вам запит у друзі", "/requests");

        public Task NewMessageAsync(int toUserId, int fromUserId, string fromUsername, int chatId)
            => SendAsync(toUserId, fromUserId, NotificationType.NewMessage,
                $"{fromUsername}: нове повідомлення", $"/chat/{chatId}");

        public Task EventJoinedAsync(int eventOwnerId, int joinerId, string joinerUsername, int eventId, string eventTitle)
            => SendAsync(eventOwnerId, joinerId, NotificationType.EventJoined,
                $"{joinerUsername} приєднався до вашої події «{eventTitle}»", "/events");

        public Task GroupJoinedAsync(int groupOwnerId, int joinerId, string joinerUsername, int groupId, string groupName)
            => SendAsync(groupOwnerId, joinerId, NotificationType.GroupJoined,
                $"{joinerUsername} вступив до групи «{groupName}»", $"/groups/{groupId}");

        public Task ChallengeCompletedAsync(int userId, string username, string challengeTitle)
            => SendAsync(userId, null, NotificationType.ChallengeCompleted,
                $"🎉 Вітаємо! Ви виконали челендж «{challengeTitle}»", "/challenges");

        private async Task TrySendEmailAsync(int userId, string message)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user?.Email == null) return;

            await _email.SendEmailAsync(
                recipientEmail: user.Email,
                subject: "Нове сповіщення від Bokado",
                body: $"Привіт, {user.Username}!\n\n{message}\n\nВідкрити Bokado: https://bokado.website",
                recipientName: user.Username
            );
        }
    }
}
