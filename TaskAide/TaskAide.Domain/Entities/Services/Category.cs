using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAide.Domain.Entities.Services
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = default!;

        public ICollection<Service> Services { get; set; } = default!;
    }
}
