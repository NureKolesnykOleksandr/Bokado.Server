using System.ComponentModel.DataAnnotations;

namespace Bokado.Server.Dtos
{
    public class EventDto
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
}
