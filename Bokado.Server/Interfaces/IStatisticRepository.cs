using Bokado.Server.Dtos;

namespace Bokado.Server.Interfaces
{
    public interface IStatisticRepository
    {
        Task<List<Dictionary<DateOnly, List<UserInfoDto>>>> GetUsersPerMonth();
        Task<Dictionary<string, int>> GetChallengesCompleted();
        Task<object> GetOverview();
    }
}
