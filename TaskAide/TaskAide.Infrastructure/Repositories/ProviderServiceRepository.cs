using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Repositories;
using TaskAide.Infrastructure.Data;

namespace TaskAide.Infrastructure.Repositories
{
    public class ProviderServiceRepository : BaseRepository<ProviderService>, IProviderServiceRepository
    {
        public ProviderServiceRepository(TaskAideContext dbContext) : base(dbContext)
        {
        }
    }
}
