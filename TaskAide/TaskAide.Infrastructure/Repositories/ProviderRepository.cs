using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Repositories;
using TaskAide.Infrastructure.Data;

namespace TaskAide.Infrastructure.Repositories
{
    public class ProviderRepository : BaseRepository<Provider>, IProviderRepository
    {
        public ProviderRepository(TaskAideContext dbContext) : base(dbContext)
        {
        }

        public async Task<Provider?> GetProviderWithUserInfoAsync(string userId)
        {
            return await _dbContext.Providers.Include(p => p.User).Include(p => p.ProviderServices).ThenInclude(ps => ps.Service).FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task<IEnumerable<Provider>> GetProvidersWithTheirServices(Expression<Func<Provider, bool>>? expression = null)
        {
            var providers = _dbContext.Providers
                .Include(p => p.ProviderServices)
                .Include(p => p.User)
                .Include(p => p.Bookings).ThenInclude(b => b.Review);

            if (expression != null)
            {
                return await providers.Where(expression).ToListAsync();
            }

            return await providers.ToListAsync();
        }

        public async Task<Provider?> GetCompanyWithAllInfoAsync(string userId)
        {
            return await _dbContext.Providers
                .Include(p => p.User)
                .Include(p => p.ProviderServices).ThenInclude(ps => ps.Service)
                .Include(p => p.Workers).ThenInclude(w => w.User)
                .FirstOrDefaultAsync(p=> p.UserId == userId);
        }
    }
}
