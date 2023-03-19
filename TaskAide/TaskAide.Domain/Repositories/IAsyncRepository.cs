using System.Linq.Expressions;

namespace TaskAide.Domain.Repositories
{
    public interface IAsyncRepository<T>
    {
        Task<T> AddAsync(T entity);

        Task<T> UpdateAsync(T entity);

        Task DeleteAsync(T entity);

        Task<T?> GetAsync(Expression<Func<T, bool>> expression);

        Task<List<T>> ListAsync(Expression<Func<T, bool>>? expression = null);

        Task DeleteListAsync(List<T> entities);
    }
}
