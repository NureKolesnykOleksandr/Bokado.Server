using Bokado.Server.Data;
using Bokado.Server.Interfaces;
using Bokado.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Bokado.Server.Repositories
{
    public class InterestRepository : IInterestRepository
    {
        private readonly SocialNetworkContext _context;

        public InterestRepository(SocialNetworkContext context)
        {
            _context = context;
        }
        public async Task<List<Interest>> GetInterests()
        {
            return await _context.Interests.ToListAsync();
        }
    }
}
