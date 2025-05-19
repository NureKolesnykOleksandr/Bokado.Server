using Bokado.Server.Dtos;
using Bokado.Server.Models;

namespace Bokado.Server.Interfaces
{
    public interface IUserRepository
    {
        Task<UserInfoDto> GetUserProfile(int userId);
        Task<UserDetailInfoDto> GetDetailedUserInfo(int userId);
        Task UpdateUserProfile(int userId, User user);
    }
}