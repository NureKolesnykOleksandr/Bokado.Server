using Bokado.Server.Dtos;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IAuthRepository
    {
        Task<AuthResultDTO> Register(RegisterDTO dto);
        Task<AuthResultDTO> Login(LoginDTO dto);
        Task<IdentityResult> ResetPassword(string email);
    }
}