using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskAide.Domain.Entities.Services
{
    public class Service : BaseEntity
    {
        public string Name { get; set; } = default!;
        public decimal DefaultPrice { get; set; } = default!;
        public int CategoryId { get; set; } = default!;
        public Category Category { get; set; } = default!;
    }
}
