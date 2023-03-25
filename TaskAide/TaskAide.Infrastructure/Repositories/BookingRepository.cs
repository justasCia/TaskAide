using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Repositories;
using TaskAide.Infrastructure.Data;

namespace TaskAide.Infrastructure.Repositories
{
    public class BookingRepository : BaseRepository<Booking>, IBookingRepository
    {
        public BookingRepository(TaskAideContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<Booking>> GetBookingsWithAllInformation(Expression<Func<Booking, bool>>? expression = null)
        {
            var bookings = _dbContext.Bookings.Include(b => b.User).Include(b => b.Provider).Include(b => b.Services).ThenInclude(bs => bs.Service).Include(b => b.MaterialPrices);

            if (expression != null)
            {
                return await bookings.Where(expression).ToListAsync();
            }

            return await bookings.ToListAsync();
        }

        public async Task RemoveMaterialPricesAsync(Booking booking)
        {
            var materialPrices = await _dbContext.BookingMaterialPrices.Where(mp => mp.BookingId == booking.Id).ToListAsync();

            _dbContext.RemoveRange(materialPrices);
            _dbContext.SaveChanges();
        }
    }
}
