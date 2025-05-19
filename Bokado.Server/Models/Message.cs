using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bokado.Server.Models
{
    [Table("messages")]
    public class Message
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MessageId { get; set; }

        [Required]
        public int ChatId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public string Text { get; set; }
        public string AttachmentUrl { get; set; }
        public bool IsGif { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public Chat Chat { get; set; }
        public User Sender { get; set; }
    }
}