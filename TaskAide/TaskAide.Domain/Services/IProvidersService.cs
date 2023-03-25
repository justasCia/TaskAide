using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.Domain.Services
{
    public interface IProvidersService
    {
        public Task<Provider?> GetProviderAsync(string userId);
        public Task<Provider?> UpsertProviderAsync(string userId, Provider provider);
        public Task<IEnumerable<Service>> PostProviderServicesAsync(string userId, IEnumerable<int> serviceIds);
        public Task<IEnumerable<Provider>> GetProvidersForBookingAsync(Booking booking);
    }
}
