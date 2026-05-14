using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Bokado.Server.Models
{
    public enum GroupStatus
    {
        Open,
        Closed
    }

    [Table("groups")]
    public class Group
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroupId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public string City { get; set; }

        public GroupStatus Status { get; set; } = GroupStatus.Open;

        [Required]
        public int CreatorId { get; set; }

        public int ChatId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User Creator { get; set; }
        public Chat Chat { get; set; }

        [JsonIgnore]
        public ICollection<GroupMember> Members { get; set; }
        [JsonIgnore]
        public ICollection<GroupInterest> GroupInterests { get; set; }
    }
}
