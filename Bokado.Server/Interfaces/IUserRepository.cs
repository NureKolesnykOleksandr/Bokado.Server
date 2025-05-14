using Bokado.Server.Dtos;
using Bokado.Server.Models;

namespace Bokado.Server.Interfaces
{
    public interface IUserRepository
    {
        Task<UserGetInfoDto> GetUserProfile(int userId);
        Task UpdateUserProfile(int userId, User user);
    }
}