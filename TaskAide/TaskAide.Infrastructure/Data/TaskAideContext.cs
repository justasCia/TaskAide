using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskAide.Domain.Entities;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.Infrastructure.Data
{
    public class TaskAideContext : IdentityDbContext<User>
    {
        public TaskAideContext(DbContextOptions<TaskAideContext> options) : base(options)
        {
        }
    }
}
