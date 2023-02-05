using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Repositories;
using TaskAide.Infrastructure.Data;

namespace TaskAide.Infrastructure.Repositories
{
    internal class UserRepository : IUserRepository
    {
        private readonly DbSet<User> _dbSet;

        public UserRepository(TaskAideContext dbContext)
        {
            _dbSet = dbContext.Set<User>();
        }

        public Task<User> AddAsync(User entity)
        {
            throw new NotImplementedException();
        }
        public Task<User> UpdateAsync(User entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(User entity)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetAsync(Expression<Func<User, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public Task<List<User>> ListAsync(Expression<Func<User, bool>> expression)
        {
            throw new NotImplementedException();
        }
    }
}
