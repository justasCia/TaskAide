using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.Domain.Entities.Bookings
{
    public class Booking : BaseEntity
    {
        public ICollection<BookingService> BookingServices { get; set; } = default!;
        public string Address { get; set; } = default!;
        public DateTime StartDate { get; set; } = default!;
        public DateTime EndDate { get; set; } = default!;
        public BookingStatus BookingStatus { get; set; 
        }
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;

        public int ProviderId { get; set; } = default!;
        public Provider Provider { get; set; } = default!;
        
        public int? ReviewId { get; set; }
        public Review? Review { get; set; }
    }
}