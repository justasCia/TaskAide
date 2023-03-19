using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Repositories;
using TaskAide.Infrastructure.Data;

namespace TaskAide.Infrastructure.Repositories
{
    public class BookingServiceRepository : BaseRepository<BookingService>, IBookingServiceRepository
    {
        public BookingServiceRepository(TaskAideContext dbContext) : base(dbContext)
        {
        }
    }
}
