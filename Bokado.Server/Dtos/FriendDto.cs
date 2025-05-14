namespace Bokado.Server.Dtos
{
    public class FriendDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
    }

    public class UserSwipeDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string AvatarUrl { get; set; }
        public string Bio { get; set; }
        public DateTime SwipedAt { get; set; }
    }
}
