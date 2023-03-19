using System.Linq.Expressions;
using TaskAide.Domain.Entities.Bookings;

namespace TaskAide.Domain.Repositories
{
    public interface IBookingRepository : IAsyncRepository<Booking>
    {
        public Task<IEnumerable<Booking>> GetBookingsWithAllInformation(Expression<Func<Booking, bool>>? expression = null);
        public Task RemoveMaterialPricesAsync(Booking booking);
    }
}
