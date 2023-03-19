using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Services;

namespace TaskAide.Domain.Services
{
    public interface IBookingService
    {
        public Task<Booking> PostBookingAsync(Booking booking);
        public Task<IEnumerable<Booking>> GetBookingsAsync(string userId, string? status);
        public Task<Booking> GetBookingAsync(int bookingId);
        public Task<Booking> UpdateBookingStatusAsync(Booking booking, string status);
        public Task<Booking> UpdateBookingServicesAsync(Booking booking, IEnumerable<BookingService> services);
        public Task<Booking> PostBookingMaterialPricesAsync(Booking booking, IEnumerable<BookingMaterialPrice> materialPrices);
    }
}
