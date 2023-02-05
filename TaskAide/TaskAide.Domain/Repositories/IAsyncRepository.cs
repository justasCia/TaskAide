using System.Collections.Generic;
using System.Linq.Expressions;
using TaskAide.Domain.Entities;

namespace TaskAide.Domain.Repositories
{
    public interface IAsyncRepository<T>
    {
        Task<T> AddAsync(T entity);

        Task<T> UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task<T?> GetAsync(Expression<Func<T, bool>> expression);

        Task<List<T>> ListAsync(Expression<Func<T, bool>> expression);

        Task DeleteListAsync(List<T> entities);
    }
}
