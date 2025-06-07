    using System.ComponentModel.DataAnnotations.Schema;
    using System.ComponentModel.DataAnnotations;

    namespace Bokado.Server.Models
    {
        [Table("event_participants")]
        public class EventParticipant
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int EventParticipantId { get; set; }

            public int EventId { get; set; }
            public int UserId { get; set; }
            public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
            public Event Event { get; set; }
            public User User { get; set; }
        }
    }