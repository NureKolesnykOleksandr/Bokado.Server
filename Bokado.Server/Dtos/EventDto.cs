using Bokado.Server.Models;
using System.ComponentModel.DataAnnotations;

namespace Bokado.Server.Dtos
{
    public class CreateEventDto
    {
        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string City { get; set; }
        public int Maximum { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }


    public class GetEventDto
    {
        public int EventId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string City { get; set; }
        public int Maximum { get; set; }
        public int CreatorId { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto Creator { get; set; }
        public ICollection<UserDto> Participants { get; set; }
    }


}
