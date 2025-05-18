using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Bokado.Server.Dtos
{
    public class UserDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        public bool IsAdmin { get; set; } = false;
    }

    public class UserInfoDto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Username { get; set; }

        public DateTime BirthDate { get; set; }

        public string? AvatarUrl { get; set; }

        public string? Bio { get; set; }

        public string? Status { get; set; }

        public int Level { get; set; } = 1;

        public string? City { get; set; }

        public bool IsPremium { get; set; } = false;

        public bool IsBanned { get; set; } = false;

        public bool IsAdmin { get; set; } = false;

        public DateTime CreatedAt { get; set; }

        public DateTime LastActive { get; set; }
    }
}
