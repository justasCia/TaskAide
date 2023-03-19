using TaskAide.API.DTOs.Auth;
using TaskAide.API.DTOs.Geometry;
using TaskAide.API.DTOs.Services;
using TaskAide.API.DTOs.Users;

namespace TaskAide.API.DTOs.Bookings
{
    public class BookingDto
    {
        public int Id { get; set; }
        public UserDto Client { get; set; } = default!;
        public ProviderDto? Provider { get; set; }
        public IEnumerable<BookingServiceDto> Services { get; set; } = Enumerable.Empty<BookingServiceDto>();
        public PointDto Address { get; set; } = default!;
        public DateTime StartDate { get; set; } = default!;
        public DateTime EndDate { get; set; } = default!;
        public string AdditionalInformation { get; set; } = default!;
        public string Status { get; set; } = default!;

        //review(nullable)
    }
}
