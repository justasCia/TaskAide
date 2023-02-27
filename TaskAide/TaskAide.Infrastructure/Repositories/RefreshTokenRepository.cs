using TaskAide.Domain.Entities.Auth;
using TaskAide.Domain.Repositories;
using TaskAide.Infrastructure.Data;

namespace TaskAide.Infrastructure.Repositories
{
    public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefrehTokenRepository
    {
        public RefreshTokenRepository(TaskAideContext dbContext) : base(dbContext)
        {
        }
    }
}
