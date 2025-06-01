using Bokado.Server.Models;
using System.Text.Json.Serialization;

namespace Bokado.Server.Dtos
{
    public class ChatDto
    {
        public int ChatId { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserDto SecondMember { get; set; }
    }
}
