using Microsoft.AspNetCore.Identity;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Repositories;
using TaskAide.Domain.Services;

namespace TaskAide.Infrastructure.Services
{
    public class ProvidersService : IProvidersService
    {
        private readonly UserManager<User> _userManager;
        private readonly IProviderRepository _providerRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IProviderServiceRepository _providerServiceRepository;

        public ProvidersService(UserManager<User> userManager, IProviderRepository providerRepository, IServiceRepository serviceRepository, IProviderServiceRepository providerServiceRepository)
        {
            _userManager = userManager;
            _providerRepository = providerRepository;
            _serviceRepository = serviceRepository;
            _providerServiceRepository = providerServiceRepository;
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

            var exsistingProvider = await _providerRepository.GetAsync(p => p.UserId == userId);

            if (exsistingProvider == null)
            {
                provider.User = user;
                provider = await _providerRepository.AddAsync(provider);
            } else
            {
                exsistingProvider.Description = provider.Description;
                exsistingProvider.Location = provider.Location;
                exsistingProvider.PlaceId = provider.PlaceId;
                exsistingProvider.BasePricePerHour = provider.BasePricePerHour;
                exsistingProvider.WorkingRange = provider.WorkingRange;
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
                    var providerService = await _providerServiceRepository.AddAsync(new ProviderService() { Provider = provider, Service = service });
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
                    CalculateDistance(provider.Location.Y, provider.Location.X, booking.Address.Y, booking.Address.X) < provider.WorkingRange &&
                    ProvidesRequiredServices(requiredServices, provider)
                );

            return providers;
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
