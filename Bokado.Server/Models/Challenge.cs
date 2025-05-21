
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bokado.Server.Models
{
    [Table("challenges")]
    public class Challenge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ChallengeId { get; set; }

        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public int Reward { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } = false;

        [JsonIgnore]
        public ICollection<UserChallenge> UserChallenges { get; set; }
    }
}