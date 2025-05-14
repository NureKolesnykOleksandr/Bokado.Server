using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bokado.Server.Models
{
    [Table("users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public DateTime BirthDate { get; set; }

        public string? AvatarUrl { get; set; }

        public string? Bio { get; set; }

        public string? Status { get; set; }

        public int Level { get; set; } = 1;

        public string? City { get; set; }

        public bool IsPremium { get; set; } = false;

        public bool IsBanned { get; set; } = false;

        public bool IsAdmin { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastActive { get; set; } = DateTime.UtcNow;

        public ICollection<UserInterest> UserInterests { get; set; }
        public ICollection<Friendship> Friends { get; set; }
        public ICollection<Swipe> Swipes { get; set; }
        public ICollection<ChatParticipant> ChatParticipants { get; set; }
        public ICollection<EventParticipant> EventParticipants { get; set; }
        public ICollection<UserChallenge> UserChallenges { get; set; }
        public ICollection<Message> Messages { get; set; }
        public ICollection<Event> CreatedEvents { get; set; }
    }
}
