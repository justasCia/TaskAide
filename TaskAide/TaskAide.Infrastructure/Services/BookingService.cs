using Microsoft.AspNetCore.Identity;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Repositories;
using TaskAide.Domain.Services;

namespace TaskAide.Infrastructure.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IServiceRepository _serviceRepository;
        private readonly IProviderRepository _providerRepository;
        private readonly UserManager<User> _userManager;

        public BookingService(IBookingRepository bookingRepository, IServiceRepository serviceRepository, IProviderRepository providerRepository, UserManager<User> userManager)
        {
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
            _providerRepository = providerRepository;
            _userManager = userManager;
        }

        public async Task<Booking> GetBookingAsync(int bookingId)
        {
            var booking = (await _bookingRepository.GetBookingsWithAllInformation(b => b.Id == bookingId)).FirstOrDefault();

            if (booking == null)
            {
                throw new NotFoundException("Booking not found");
            }

            return booking;
        }

        public async Task<IEnumerable<Booking>> GetBookingsAsync(string userId, string? status)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            if (!user.IsProvider)
            {
                if (status != null)
                {
                    if (!Enum.TryParse(status, out BookingStatus bookingStatus))
                    {
                        throw new BadRequestException("Invalid booking status");
                    }

                    return (await _bookingRepository.GetBookingsWithAllInformation(b => b.UserId == userId && b.Status == bookingStatus)).OrderByDescending(b => b.Id);
                }

                return (await _bookingRepository.GetBookingsWithAllInformation(b => b.UserId == userId)).OrderByDescending(b => b.Id);
            }

            var provider = await _providerRepository.GetAsync(p => p.UserId == user.Id);

            if (status != null)
            {
                if (!Enum.TryParse(status, out BookingStatus bookingStatus))
                {
                    throw new BadRequestException("Invalid booking status");
                }

                return (await _bookingRepository.GetBookingsWithAllInformation(b => b.ProviderId == provider!.Id && b.Status == bookingStatus)).OrderByDescending(b => b.Id);
            }

            return (await _bookingRepository.GetBookingsWithAllInformation(b => b.ProviderId == provider!.Id)).OrderByDescending(b => b.Id);
        }

        public async Task<Booking> PostBookingAsync(Booking booking)
        {
            foreach (var service in booking.BookingServices)
            {
                if (await _serviceRepository.GetAsync(s => s.Id == service.ServiceId) == null)
                {
                    throw new NotFoundException($"Service {service.ServiceId} not found.");
                }
            }

            if (await _providerRepository.GetAsync(p => p.Id == booking.ProviderId) == null)
            {
                throw new NotFoundException($"Provider not found.");
            }

            if (await _userManager.FindByIdAsync(booking.UserId) == null)
            {
                throw new NotFoundException($"User not found.");
            }

            return await _bookingRepository.AddAsync(booking);
        }

        public async Task<Booking> UpdateBookingStatus(int bookingId, string status)
        {
            if (!Enum.TryParse(status, out BookingStatus bookingStatus))
            {
                throw new BadRequestException("Invalid booking status");
            }

            var booking = await _bookingRepository.GetAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                throw new NotFoundException("Booking not found");
            }

            booking.Status = bookingStatus;

            return await _bookingRepository.UpdateAsync(booking);
        }
    }
}
