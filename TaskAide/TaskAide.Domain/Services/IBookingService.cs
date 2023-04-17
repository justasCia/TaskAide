using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Services;

namespace TaskAide.Domain.Services
{
    public interface IBookingService
    {
        public Task<Booking> PostBookingAsync(Booking booking);
        public Task<IEnumerable<Booking>> GetBookingsAsync(string userId, string? status = null, bool? paid = null);
        public Task<Booking> GetBookingAsync(int bookingId);
        public Task<Booking> UpdateBookingStatusAsync(Booking booking, string status);
        public Task<Booking> UpdateBookingServicesAsync(Booking booking, IEnumerable<BookingService> services);
        public Task<Booking> PostBookingMaterialPricesAsync(Booking booking, IEnumerable<BookingMaterialPrice> materialPrices);
        public Task<Booking> AssignBookingWorkerAsync(Booking booking, int workerId);
        public Task<Booking> AddBookingReviewAsync(Booking booking, Review review);
    }
}
