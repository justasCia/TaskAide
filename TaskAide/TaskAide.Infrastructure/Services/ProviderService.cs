using Microsoft.AspNetCore.Identity;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Reports;
using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Repositories;
using TaskAide.Domain.Services;

namespace TaskAide.Infrastructure.Services
{
    public class ProviderService : IProviderService
    {
        private readonly UserManager<User> _userManager;
        private readonly IProviderRepository _providerRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IProviderServiceRepository _providerServiceRepository;
        private readonly IBookingRepository _bookingRepository;

        public ProviderService(UserManager<User> userManager, IProviderRepository providerRepository, IServiceRepository serviceRepository, IProviderServiceRepository providerServiceRepository, IBookingRepository bookingRepository)
        {
            _userManager = userManager;
            _providerRepository = providerRepository;
            _serviceRepository = serviceRepository;
            _providerServiceRepository = providerServiceRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<Provider?> GetProviderAsync(string userId)
        {
            return await _providerRepository.GetProviderWithUserInfoAsync(userId);
        }

        public async Task<Provider?> UpsertProviderAsync(string userId, Provider provider)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("user not found");
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var exsistingProvider = await _providerRepository.GetAsync(p => p.UserId == userId);

            if (exsistingProvider == null)
            {
                provider.User = user;
                provider.IsCompany = userRoles.Contains(Roles.Company);
                provider = await _providerRepository.AddAsync(provider);
            }
            else
            {
                exsistingProvider.Description = provider.Description;
                exsistingProvider.Location = provider.Location;
                exsistingProvider.PlaceId = provider.PlaceId;
                exsistingProvider.BasePricePerHour = provider.BasePricePerHour;
                exsistingProvider.WorkingRange = provider.WorkingRange;
                exsistingProvider.IsCompany = userRoles.Contains(Roles.Company);
                provider = await _providerRepository.UpdateAsync(exsistingProvider);
            }

            return provider;
        }

        public async Task<IEnumerable<Service>> PostProviderServicesAsync(string userId, IEnumerable<int> serviceIds)
        {
            var provider = await GetProviderAsync(userId);

            if (provider == null)
            {
                throw new NotFoundException("provider not found");
            }

            var providerServices = await _providerServiceRepository.ListAsync(ps => ps.ProviderId == provider.Id);
            await _providerServiceRepository.DeleteListAsync(providerServices);

            var servicesAdded = new List<Service>();

            foreach (var serviceId in serviceIds)
            {
                var service = await _serviceRepository.GetAsync(s => s.Id == serviceId);

                if (service != null)
                {
                    var providerService = await _providerServiceRepository.AddAsync(new Domain.Entities.Services.ProviderService() { Provider = provider, Service = service });
                    servicesAdded.Add(service);
                }
            }

            return servicesAdded;
        }

        public async Task<IEnumerable<Provider>> GetProvidersForBookingAsync(Booking booking)
        {
            var requiredServices = booking.Services.Select(bs => bs.ServiceId);

            var providers = (await _providerRepository.GetProvidersWithTheirServices())
                .Where(provider =>
                    !string.IsNullOrEmpty(provider.AccountId) &&
                    IsSuitableCompany(provider) &&
                    provider.CompanyId == null &&
                    ProvidesRequiredServices(requiredServices, provider) &&
                    CalculateDistance(provider.Location!.Y, provider.Location.X, booking.Address.Y, booking.Address.X) < provider.WorkingRange
                );

            return providers;
        }

        public async Task<IEnumerable<Provider>> GetCompanyWorkersAsync(string userId)
        {
            var company = await _providerRepository.GetCompanyWithAllInfoAsync(userId);

            if (company == null)
            {
                throw new NotFoundException("Company not found");
            }

            return company.Workers;
        }

        public async Task<Provider> AddCompanyWorkerAsync(string userId, User user)
        {
            var company = await _providerRepository.GetCompanyWithAllInfoAsync(userId);

            if (company == null)
            {
                throw new NotFoundException("Company not found");
            }

            var worker = new Provider()
            {
                User = user,
                Company = company
            };

            return await _providerRepository.AddAsync(worker);

        }

        public async Task<ProviderReport> GetProviderReportAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var provider = await _providerRepository.GetAsync(p => p.UserId == userId);

            Func<Booking, bool> dateFilter = DateFilter(startDate, endDate);

            var bookings = await _bookingRepository.GetBookingsWithAllInformation(booking =>
                booking.Provider.UserId == userId);

            bookings = bookings.Where(dateFilter);

            var bookingsWithRevenue = bookings.Where(booking => booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.CancelledWithPartialPayment);

            List<WorkerReport>? workerReports = null;
            if (provider!.IsCompany)
            {
                var workers = bookings.Select(b => b.Worker)
                    .Where(worker => worker != null)
                    .DistinctBy(worker => worker!.Id);

                workerReports = GetWorkersReport(bookings, workers!);
            }

            return new ProviderReport()
            {
                MaterialsCost = bookingsWithRevenue.Sum(b => b.CalculateMaterialsCost()),
                ServicesRevenue = bookingsWithRevenue.Sum(b => b.CalculateServicesCost()),
                TotalIncome = bookingsWithRevenue.Sum(b => b.CalculateTotalCost()),
                RevenueFromEachService = GetServiceRevenueByType(bookingsWithRevenue),
                BookingRequests = bookings.Count(),
                BookingRequestsCancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                BookingRequestsCancelledWithPartialPayment = bookingsWithRevenue.Count(b => b.Status == BookingStatus.CancelledWithPartialPayment),
                BookingRequestsCompleted = bookingsWithRevenue.Count(b => b.Status == BookingStatus.Completed),
                FavouriteBookingRequest = bookings.SelectMany(booking => booking.Services).GroupBy(s => s.Service.Name).OrderByDescending(g => g.Count()).FirstOrDefault()?.Key!,
                WorkerReports = workerReports
            };
        }

        public async Task<WorkerReport> GetWorkerReportAsync(string userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            Func<Booking, bool> dateFilter = DateFilter(startDate, endDate);

            var bookings = await _bookingRepository.GetBookingsWithAllInformation(booking =>
                booking.Worker != null && booking.Worker.UserId == userId);

            bookings = bookings.Where(dateFilter);

            var bookingsWithRevenue = bookings.Where(booking => booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.CancelledWithPartialPayment);

            var workerReport = new WorkerReport()
            {
                ServicesRevenue = bookingsWithRevenue.Sum(b => b.CalculateServicesCost()),
                RevenueFromEachService = GetServiceRevenueByType(bookingsWithRevenue),
                BookingRequests = bookings.Count(),
                BookingRequestsCancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled),
                BookingRequestsCancelledWithPartialPayment = bookingsWithRevenue.Count(b => b.Status == BookingStatus.CancelledWithPartialPayment),
                BookingRequestsCompleted = bookingsWithRevenue.Count(b => b.Status == BookingStatus.Completed)
            };

            return workerReport;
        }

        private static Func<Booking, bool> DateFilter(DateTime? startDate, DateTime? endDate)
        {
            return (Booking booking) =>
            {
                if (startDate != null && endDate != null)
                {
                    return (booking.StartDate >= startDate || booking.EndDate >= startDate) &&
                    (booking.StartDate <= endDate || booking.EndDate <= endDate);
                }
                else if (startDate != null)
                {
                    return booking.StartDate >= startDate || booking.EndDate >= startDate;
                }
                else if (endDate != null)
                {
                    return booking.EndDate <= endDate || booking.EndDate <= endDate;
                }
                else
                {
                    return true;
                }
            };
        }

        private static List<WorkerReport> GetWorkersReport(IEnumerable<Booking> bookings, IEnumerable<Provider> workers)
        {
            List<WorkerReport>? workerReports = new List<WorkerReport>();
            foreach (var worker in workers)
            {
                var workerBookigs = bookings.Where(b => b.WorkerId == worker!.Id);
                var workerBookingsWithRevenue = workerBookigs.Where(booking => booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.CancelledWithPartialPayment);
                var workerReport = new WorkerReport()
                {
                    FirstName = worker!.User!.FirstName!,
                    LastName = worker.User.LastName!,
                    ServicesRevenue = workerBookingsWithRevenue.Sum(b => b.CalculateServicesCost()),
                    RevenueFromEachService = GetServiceRevenueByType(workerBookingsWithRevenue),
                    BookingRequests = workerBookigs.Count(),
                    BookingRequestsCancelled = workerBookigs.Count(b => b.Status == BookingStatus.Cancelled),
                    BookingRequestsCancelledWithPartialPayment = workerBookingsWithRevenue.Count(b => b.Status == BookingStatus.CancelledWithPartialPayment),
                    BookingRequestsCompleted = workerBookingsWithRevenue.Count(b => b.Status == BookingStatus.Completed)
                };
                workerReports.Add(workerReport);
            }

            return workerReports;
        }

        private static IDictionary<string, decimal> GetServiceRevenueByType(IEnumerable<Booking> bookings)
        {
            var serviceRevenueByType = new Dictionary<string, decimal>();

            foreach (var bookingService in bookings.SelectMany(b => b.Services))
            {
                if (!serviceRevenueByType.ContainsKey(bookingService.Service.Name))
                {
                    serviceRevenueByType[bookingService.Service.Name] = 0m;
                }
                serviceRevenueByType[bookingService.Service.Name] += bookingService.Price;
            }

            return serviceRevenueByType;
        }

        private static bool IsSuitableCompany(Provider provider)
        {
            if (!provider.IsCompany) return true;

            return
                provider.Workers != null && provider.Workers.Any();

        }

        private static bool ProvidesRequiredServices(IEnumerable<int> requiredServices, Provider provider)
        {
            var providerServices = provider.ProviderServices.Select(ps => ps.ServiceId);
            return requiredServices.All(rs => providerServices.Contains(rs));
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double radius = 6371; // Earth's radius in kilometers
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distance = radius * c;
            return distance;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
