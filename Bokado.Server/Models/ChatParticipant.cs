using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bokado.Server.Models
{
    [Table("chat_participants")]
    public class ChatParticipant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChatParticipantId { get; set; }

        public int ChatId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public Chat Chat { get; set; }
        public User User { get; set; }
    }
}
