using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Services;

namespace TaskAide.Domain.Entities.Users
{
    public class Provider : BaseEntity
    {
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;

        public string Description { get; set; } = default!;

       
        public ICollection<Booking> Bookings { get; set; } = default!;
        public ICollection<ProviderService> ProviderServices { get; set; } = default!;
    }
}
