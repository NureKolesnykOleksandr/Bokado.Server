using Bokado.Server.Dtos;
using Bokado.Server.Models;

namespace Bokado.Server.Interfaces
{
    public interface IStatisticRepository
    {
        Task<List<Dictionary<DateOnly, List<UserInfoDto>>>> GetUsersPerMonth();
        Task<Dictionary<string, int>> GetChallengesCompleted();
    }
}
