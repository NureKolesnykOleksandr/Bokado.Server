namespace Bokado.Server.Dtos
{
    public class FriendDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
    }

    public class FriendRequestDto
    {
        public int FriendRequestId { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public string? City { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class FriendStatusDto
    {
        public string Status { get; set; }
    }
}
