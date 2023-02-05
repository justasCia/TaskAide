﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskAide.Domain.Entities;
using TaskAide.Domain.Entities.Auth;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.Infrastructure.Data
{
    public class TaskAideContext : IdentityDbContext<User>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public TaskAideContext(DbContextOptions<TaskAideContext> options) : base(options)
        {
        }
    }
}
