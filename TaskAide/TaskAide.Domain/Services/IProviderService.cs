using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Reports;
using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.Domain.Services
{
    public interface IProviderService
    {
        public Task<Provider?> GetProviderAsync(string userId);
        public Task<Provider?> UpsertProviderAsync(string userId, Provider provider);
        public Task<IEnumerable<Service>> PostProviderServicesAsync(string userId, IEnumerable<int> serviceIds);
        public Task<IEnumerable<Provider>> GetProvidersForBookingAsync(Booking booking);
        public Task<IEnumerable<Provider>> GetCompanyWorkersAsync(string userId);
        public Task<Provider> AddCompanyWorkerAsync(string userId, User user);
        public Task<ProviderReport> GetProviderReportAsync(string userId);
        public Task<WorkerReport> GetWorkerReportAsync(string userId);
    }
}
