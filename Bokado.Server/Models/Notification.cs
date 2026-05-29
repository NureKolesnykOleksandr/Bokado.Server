using System.ComponentModel.DataAnnotations.Schema;

namespace Bokado.Server.Models
{
    [Table("notifications")]
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public int? ActorId { get; set; }
        public NotificationType Type { get; set; }
        public string Message { get; set; } = "";
        public string? Link { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? User { get; set; }
        public User? Actor { get; set; }
    }

    public enum NotificationType
    {
        FriendRequest = 1,
        NewMessage = 2,
        EventJoined = 3,
        GroupJoined = 4,
        ChallengeCompleted = 5,
    }
}
