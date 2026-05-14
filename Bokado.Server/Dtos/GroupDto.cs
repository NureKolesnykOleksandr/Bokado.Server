using Bokado.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace Bokado.Server.Dtos
{
    public class CreateGroupDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required]
        public string City { get; set; }

        public List<int> InterestIds { get; set; } = new();
    }

    public class UpdateGroupDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? City { get; set; }
    }

    public class GroupInterestDto
    {
        public int InterestId { get; set; }
        public string Name { get; set; }
    }

    public class GroupMemberDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string? AvatarUrl { get; set; }
        public GroupMemberRole Role { get; set; }
    }

    public class GetGroupDto
    {
        public int GroupId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string City { get; set; }
        public GroupStatus Status { get; set; }
        public int CreatorId { get; set; }
        public int ChatId { get; set; }
        public int MaxMembers { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto Creator { get; set; }
        public List<GroupMemberDto> Members { get; set; }
        public List<GroupInterestDto> Interests { get; set; }
    }
}
