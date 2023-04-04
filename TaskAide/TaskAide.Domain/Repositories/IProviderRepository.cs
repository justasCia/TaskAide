using System.Linq.Expressions;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.Domain.Repositories
{
    public interface IProviderRepository : IAsyncRepository<Provider>
    {
        public Task<Provider?> GetProviderWithUserInfoAsync(string userId);
        public Task<IEnumerable<Provider>> GetProvidersWithTheirServices(Expression<Func<Provider, bool>>? expression = null);
        public Task<Provider?> GetCompanyWithAllInfoAsync(string userId);
    }
}
