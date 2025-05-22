using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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

        [JsonIgnore]
        public ICollection<UserInterest>? UserInterests { get; set; }
        [JsonIgnore]
        public ICollection<Friendship>? Friends { get; set; }
        [JsonIgnore]
        public ICollection<Swipe>? Swipes { get; set; }
        [JsonIgnore]
        public ICollection<ChatParticipant>? ChatParticipants { get; set; }
        [JsonIgnore]
        public ICollection<EventParticipant>? EventParticipants { get; set; }
        [JsonIgnore]
        public ICollection<UserChallenge>? UserChallenges { get; set; }
        [JsonIgnore]
        public ICollection<Message>? Messages { get; set; }
        [JsonIgnore]
        public ICollection<Event>? CreatedEvents { get; set; }
    }
}
