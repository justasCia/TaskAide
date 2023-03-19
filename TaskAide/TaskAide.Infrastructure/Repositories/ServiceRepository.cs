using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Repositories;
using TaskAide.Infrastructure.Data;

namespace TaskAide.Infrastructure.Repositories
{
    public class ServiceRepository : BaseRepository<Service>, IServiceRepository
    {
        public ServiceRepository(TaskAideContext dbContext) : base(dbContext)
        {
        }
    }
}
