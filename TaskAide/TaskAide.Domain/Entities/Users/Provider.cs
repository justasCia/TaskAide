using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAide.Domain.Entities.Bookings;

namespace TaskAide.Domain.Entities.Users
{
    public class Provider : BaseEntity
    {
        public string Description { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;
        public ICollection<Booking> Bookings { get; set; } = default!;
    }
}
