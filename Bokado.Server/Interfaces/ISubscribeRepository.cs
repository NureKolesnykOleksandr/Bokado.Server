using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface ISubscribeRepository
    {
        Task<IdentityResult> GiveSubscribe(int userId);
        Task<IdentityResult> TakeSubscribe(int userId);
    }
}
