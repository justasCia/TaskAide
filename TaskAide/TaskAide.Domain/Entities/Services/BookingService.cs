using TaskAide.Domain.Entities.Bookings;

namespace TaskAide.Domain.Entities.Services
{
    public class BookingService : BaseEntity
    {
        public int ServiceId { get; set; }
        public Service Service { get; set; } = default!;

        public int BookingId { get; set; }
        public Booking Booking { get; set; } = default!;
    }
}
