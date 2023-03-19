using TaskAide.API.DTOs.Services;

namespace TaskAide.API.DTOs.Bookings
{
    public class BookingServiceDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int HoursOfWork { get; set; }
        public ServiceDto Service { get; set; } = default!;
    }
}
