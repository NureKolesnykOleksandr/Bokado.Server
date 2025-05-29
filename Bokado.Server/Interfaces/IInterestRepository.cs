using Bokado.Server.Models;

namespace Bokado.Server.Interfaces
{
    public interface IInterestRepository
    {
        public Task<List<Interest>> GetInterests();
    }
}
