using TaskAide.Domain.Entities.Users;

namespace TaskAide.Domain.Services
{
    public interface IUserService
    {
        public Task<User> GetUserAsync(string userId);
    }
}
