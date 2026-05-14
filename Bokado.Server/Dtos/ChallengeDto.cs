namespace Bokado.Server.Dtos
{
    public class ChallengeDto
    {
        public int ChallengeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Reward { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public double CompletionRate { get; set; }
    }
}
