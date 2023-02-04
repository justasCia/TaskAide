using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskAide.Domain.Entities;
using TaskAide.Domain.Repositories;
using TaskAide.Infrastructure.Data;

namespace TaskAide.Infrastructure.Repositories
{
    public class BaseRepository<T> : IAsyncRepository<T> where T : BaseEntity
    {
        private readonly TaskAideContext _dbContext;

        public BaseRepository(TaskAideContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbContext.AddAsync(entity);
            await SaveChanges();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _dbContext.Update(entity);
            await SaveChanges();

            return entity;
        }

        public async Task DeleteAsync(T entity)
        {
            _dbContext.Remove(entity);
            await SaveChanges();
        }

        public async Task<T?> GetAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbContext.Set<T>().FirstOrDefaultAsync(expression);
        }

        public async Task<List<T>> ListAsync(Expression<Func<T, bool>> expression)
        {
            return await _dbContext.Set<T>().ToListAsync();
        }

        private async Task SaveChanges()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
