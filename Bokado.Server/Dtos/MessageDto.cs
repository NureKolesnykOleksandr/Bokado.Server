using Microsoft.AspNetCore.Mvc;

namespace Bokado.Server.Dtos
{
    public class MessageDto
    {
        public int ToId { get; set; }
        public string? Text { get; set; }
        public IFormFile? attachedFile { get; set; }
    }

    public class SendMessageDto
    {
        public IFormFile? UserIcon { get; set; }
        public string Text { get; set; }
        public int ToId { get; set; }
    }
}
