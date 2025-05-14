
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bokado.Server.Models
{
    [Table("events")]
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string City { get; set; }

        [Required]
        public int CreatorId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User Creator { get; set; }
        public ICollection<EventParticipant> Participants { get; set; }
    }
}