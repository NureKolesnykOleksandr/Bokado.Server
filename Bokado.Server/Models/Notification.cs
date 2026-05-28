namespace Bokado.Server.Models
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }          // кому
        public int? ActorId { get; set; }        // хто спровокував (може бути null для системних)
        public NotificationType Type { get; set; }
        public string Message { get; set; } = "";
        public string? Link { get; set; }        // /chats/5, /events/3 тощо
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
        public User? Actor { get; set; }
    }

    public enum NotificationType
    {
        FriendRequest = 1,
        NewMessage    = 2,
        EventJoined   = 3,
        GroupJoined   = 4,
        ChallengeCompleted = 5,
    }
}
