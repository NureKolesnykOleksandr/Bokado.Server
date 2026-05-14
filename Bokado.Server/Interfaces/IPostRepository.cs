using Bokado.Server.Dtos;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IPostRepository
    {
        Task<List<GetPostDto>> GetUserPosts(int profileUserId, int currentUserId);
        Task<GetPostDto> CreatePost(CreatePostDto dto, int userId);
        Task<IdentityResult> DeletePost(int postId, int userId);
        Task<IdentityResult> LikePost(int postId, int userId);
        Task<IdentityResult> UnlikePost(int postId, int userId);
    }
}
