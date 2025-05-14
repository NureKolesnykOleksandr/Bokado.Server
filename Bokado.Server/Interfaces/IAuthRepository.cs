using Bokado.Server.Dtos;

namespace Bokado.Server.Interfaces
{
    public interface IAuthRepository
    {
        Task<AuthResultDTO> Register(RegisterDTO dto);
        Task<AuthResultDTO> Login(LoginDTO dto);
        Task<bool> ResetPassword(string email);
    }
}