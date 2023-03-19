using TaskAide.API.DTOs.Geometry;

namespace TaskAide.API.DTOs.Bookings
{
    public class BookingRequestDto
    {
        public ICollection<BookingServiceDto> Services { get; set; } = default!;
        public PointDto Address { get; set; } = default!;
        public DateTime StartDate { get; set; } = default!;
        public DateTime EndDate { get; set; } = default!;
        public string AdditionalInformation { get; set; } = default!;
    }
}
