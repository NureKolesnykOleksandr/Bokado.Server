namespace Bokado.Server.Dtos
{
    public class MessageDto
    {
        public int FromId { get; set; }
        public int ToId { get; set; }
        public string Message { get; set; }
        public IFormFile? attachedFile { get; set; }
    }
}
