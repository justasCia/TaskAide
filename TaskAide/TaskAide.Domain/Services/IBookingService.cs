using TaskAide.Domain.Entities.Bookings;

namespace TaskAide.Domain.Services
{
    public interface IBookingService
    {
        public Task<Booking> PostBookingAsync(Booking booking);
        public Task<IEnumerable<Booking>> GetBookingsAsync(string userId, string? status);
        public Task<Booking> GetBookingAsync(int bookingId);
        public Task<Booking> UpdateBookingStatus(int bookingId, string status);
    }
}
