using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Repositories;
using TaskAide.Infrastructure.Data;

namespace TaskAide.Infrastructure.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(TaskAideContext dbContext) : base(dbContext)
        {
        }
    }
}
