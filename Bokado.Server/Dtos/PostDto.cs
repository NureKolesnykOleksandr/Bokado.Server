using System.ComponentModel.DataAnnotations;

namespace Bokado.Server.Dtos
{
    public class CreatePostDto
    {
        public string? Text { get; set; }
        public IFormFile? Image { get; set; }
        public IFormFile? Video { get; set; }
    }

    public class GetPostDto
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string AuthorUsername { get; set; }
        public string? AuthorAvatarUrl { get; set; }
        public string? Text { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LikesCount { get; set; }
        public bool IsLikedByMe { get; set; }
    }
}
