namespace Bokado.Server.Dtos
{
    public class ChatMemberDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class LastMessageDto
    {
        public int MessageId { get; set; }
        public string Text { get; set; }
        public string? Attachment { get; set; }
        public DateTime SentAt { get; set; }
        public int SenderId { get; set; }
        public bool IsRead { get; set; }
    }

    public class ChatDto
    {
        public int ChatId { get; set; }
        public DateTime CreatedAt { get; set; }
        public ChatMemberDto? SecondMember { get; set; }
        public bool IsGroup { get; set; } = false;
        public int? GroupId { get; set; }
        public string? GroupName { get; set; }
        public LastMessageDto? LastMessage { get; set; }
        public int UnreadCount { get; set; } = 0;
    }
}
