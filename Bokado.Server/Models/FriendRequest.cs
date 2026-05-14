using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bokado.Server.Models
{
    [Table("friend_requests")]
    public class FriendRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FriendRequestId { get; set; }

        [Required]
        public int FromUserId { get; set; }

        [Required]
        public int ToUserId { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public User FromUser { get; set; }
        public User ToUser { get; set; }
    }
}
