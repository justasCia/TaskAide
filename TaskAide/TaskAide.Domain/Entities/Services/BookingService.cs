using System.ComponentModel.DataAnnotations.Schema;
using TaskAide.Domain.Entities.Bookings;

namespace TaskAide.Domain.Entities.Services
{
    public class BookingService : BaseEntity
    {
        [Column(TypeName = "decimal(6,2)")]
        public decimal Price { get; set; }
        public int HoursOfWork { get; set; }

        public int ServiceId { get; set; }
        public Service Service { get; set; } = default!;

        public int BookingId { get; set; }
        public Booking Booking { get; set; } = default!;
    }
}
