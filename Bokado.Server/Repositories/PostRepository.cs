using Bokado.Server.Data;
using Bokado.Server.Dtos;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Bokado.Server.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Bokado.Server.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly SocialNetworkContext _context;
        private readonly FileService _fileService;

        public PostRepository(SocialNetworkContext context, FileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        public async Task<List<GetPostDto>> GetUserPosts(int profileUserId, int currentUserId)
        {
            var posts = await _context.Posts
                .Where(p => p.UserId == profileUserId)
                .Include(p => p.Author)
                .Include(p => p.Likes)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return posts.Select(p => new GetPostDto
            {
                PostId = p.PostId,
                UserId = p.UserId,
                AuthorUsername = p.Author.Username,
                AuthorAvatarUrl = p.Author.AvatarUrl,
                Text = p.Text,
                ImageUrl = p.ImageUrl,
                CreatedAt = p.CreatedAt,
                LikesCount = p.Likes.Count,
                IsLikedByMe = p.Likes.Any(l => l.UserId == currentUserId)
            }).ToList();
        }

        public async Task<GetPostDto> CreatePost(CreatePostDto dto, int userId)
        {
            if (string.IsNullOrWhiteSpace(dto.Text) && dto.Image == null && dto.Video == null)
                throw new ArgumentException("Пост повинен містити текст, зображення або відео");

            string? mediaUrl = null;

            if (dto.Image != null)
            {
                var imageExtensions = new[] { ".png", ".jpg", ".jpeg", ".webp" };
                mediaUrl = await _fileService.SaveFileAsync(dto.Image, "bokado/posts", imageExtensions,
                    Path.GetFileNameWithoutExtension(dto.Image.FileName));
            }
            else if (dto.Video != null)
            {
                var videoExtensions = new[] { ".mp4", ".webm", ".mov", ".avi" };
                mediaUrl = await _fileService.SaveFileAsync(dto.Video, "bokado/videos", videoExtensions,
                    Path.GetFileNameWithoutExtension(dto.Video.FileName));
            }

            var post = new Post
            {
                UserId = userId,
                Text = dto.Text,
                ImageUrl = mediaUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var author = await _context.Users.FindAsync(userId);

            return new GetPostDto
            {
                PostId = post.PostId,
                UserId = post.UserId,
                AuthorUsername = author!.Username,
                AuthorAvatarUrl = author.AvatarUrl,
                Text = post.Text,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                LikesCount = 0,
                IsLikedByMe = false
            };
        }

        public async Task<IdentityResult> DeletePost(int postId, int userId)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
                return IdentityResult.Failed(new IdentityError { Description = "Пост не знайдено" });

            var user = await _context.Users.FindAsync(userId);
            if (post.UserId != userId && !(user?.IsAdmin ?? false))
                return IdentityResult.Failed(new IdentityError { Description = "Немає прав для видалення цього поста" });

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> LikePost(int postId, int userId)
        {
            if (!await _context.Posts.AnyAsync(p => p.PostId == postId))
                return IdentityResult.Failed(new IdentityError { Description = "Пост не знайдено" });

            if (await _context.PostLikes.AnyAsync(l => l.PostId == postId && l.UserId == userId))
                return IdentityResult.Failed(new IdentityError { Description = "Ви вже лайкнули цей пост" });

            _context.PostLikes.Add(new PostLike { PostId = postId, UserId = userId });
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UnlikePost(int postId, int userId)
        {
            var like = await _context.PostLikes.FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (like == null)
                return IdentityResult.Failed(new IdentityError { Description = "Ви не лайкали цей пост" });

            _context.PostLikes.Remove(like);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
    }
}
