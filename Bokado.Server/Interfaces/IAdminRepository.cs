﻿using Bokado.Server.Dtos;
using Bokado.Server.Models;
using Microsoft.AspNetCore.Identity;

namespace Bokado.Server.Interfaces
{
    public interface IAdminRepository
    {
        Task<IdentityResult> BanUser(int userId);
        Task<IdentityResult> UnbanUser(int userId);
        Task<IdentityResult> SelectChallenges(ICollection<int> challengeIds);
        Task<IEnumerable<Challenge>> GetaAllChallenges();
        Task<IEnumerable<UserDetailInfoDto>> GetaAllUsers();

    }
}
