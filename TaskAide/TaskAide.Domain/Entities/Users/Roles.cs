using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAide.Domain.Entities.Users
{
    public static class Roles
    {
        public const string Admin = nameof(Admin);
        public const string Client = nameof(Client);
        public const string Worker = nameof(Worker);

        public static readonly IReadOnlyCollection<string> All = new[] { Admin, Client, Worker };
    }
}
