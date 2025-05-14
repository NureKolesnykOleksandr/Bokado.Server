using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bokado.Server.Models
{
    [Table("swipes")]
    public class Swipe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SwipeId { get; set; }

        [Required]
        public int SwiperId { get; set; }

        [Required]
        public int TargetUserId { get; set; }

        [Required]
        public string Action { get; set; } // "like" or "pass"

        public DateTime SwipedAt { get; set; } = DateTime.UtcNow;

        public bool IsMatch { get; set; } = false;

        public User Swiper { get; set; }
        public User TargetUser { get; set; }
    }
}