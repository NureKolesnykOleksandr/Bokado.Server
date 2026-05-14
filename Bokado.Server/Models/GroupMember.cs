using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bokado.Server.Models
{
    public enum GroupMemberRole { Owner, Admin, Member }

    [Table("group_members")]
    public class GroupMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int GroupMemberId { get; set; }

        public int GroupId { get; set; }
        public int UserId { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public GroupMemberRole Role { get; set; } = GroupMemberRole.Member;

        public Group Group { get; set; }
        public User User { get; set; }
    }
}
