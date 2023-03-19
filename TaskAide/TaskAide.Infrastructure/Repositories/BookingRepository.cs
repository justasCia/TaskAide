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
            var bookings = _dbContext.Bookings.Include(b => b.User).Include(b => b.Provider).Include(b => b.BookingServices).ThenInclude(bs => bs.Service);

            if (expression != null)
            {
                return await bookings.Where(expression).ToListAsync();
            }

            return await bookings.ToListAsync();
        }
    }
}
