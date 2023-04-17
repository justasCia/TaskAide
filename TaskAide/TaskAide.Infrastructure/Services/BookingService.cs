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
        private readonly IBookingServiceRepository _bookingServiceRepository;
        private readonly UserManager<User> _userManager;

        public BookingService(IBookingRepository bookingRepository, IServiceRepository serviceRepository, IProviderRepository providerRepository, IBookingServiceRepository bookingServiceRepository, UserManager<User> userManager)
        {
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
            _providerRepository = providerRepository;
            _bookingServiceRepository = bookingServiceRepository;
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

        public async Task<IEnumerable<Booking>> GetBookingsAsync(string userId, string? status = null, bool? paid = null)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            IOrderedEnumerable<Booking>? bookings;

            if (!user.IsProvider)
            {
                if (status != null)
                {
                    if (!Enum.TryParse(status, out BookingStatus bookingStatus))
                    {
                        if (status == "done")
                        {
                            bookings = (await _bookingRepository.GetBookingsWithAllInformation(b => b.UserId == userId && (b.Status == BookingStatus.Completed || b.Status == BookingStatus.CancelledWithPartialPayment))).OrderByDescending(b => b.Id);
                            return ReturnBookings(paid, bookings);
                        }
                        throw new BadRequestException("Invalid booking status");
                    }

                    bookings = (await _bookingRepository.GetBookingsWithAllInformation(b => b.UserId == userId && b.Status == bookingStatus)).OrderByDescending(b => b.Id);
                    return ReturnBookings(paid, bookings);
                }

                bookings = (await _bookingRepository.GetBookingsWithAllInformation(b => b.UserId == userId)).OrderByDescending(b => b.Id);
                return ReturnBookings(paid, bookings);
            }

            var provider = await _providerRepository.GetAsync(p => p.UserId == user.Id);

            if (provider != null && provider.CompanyId != null)
            {
                if (status != null)
                {
                    if (!Enum.TryParse(status, out BookingStatus bookingStatus))
                    {
                        if (status == "done")
                        {
                            bookings = (await _bookingRepository.GetBookingsWithAllInformation(b => b.WorkerId == provider!.Id && (b.Status == BookingStatus.Completed || b.Status == BookingStatus.CancelledWithPartialPayment))).OrderByDescending(b => b.Id);
                            return ReturnBookings(paid, bookings);
                        }
                        throw new BadRequestException("Invalid booking status");
                    }

                    bookings = (await _bookingRepository.GetBookingsWithAllInformation(b => b.WorkerId == provider!.Id && b.Status == bookingStatus)).OrderByDescending(b => b.Id);
                    return ReturnBookings(paid, bookings);
                }

                bookings = (await _bookingRepository.GetBookingsWithAllInformation(b => b.WorkerId == provider!.Id)).OrderByDescending(b => b.Id);
                return ReturnBookings(paid, bookings);
            }

            if (provider == null || string.IsNullOrEmpty(provider.BankAccount))
            {
                throw new BadRequestException("User cannot accept bookings until provider information filled");
            }

            if (status != null)
            {
                if (!Enum.TryParse(status, out BookingStatus bookingStatus))
                {
                    if (status == "done")
                    {
                        bookings = (await _bookingRepository.GetBookingsWithAllInformation(b => b.ProviderId == provider!.Id && (b.Status == BookingStatus.Completed || b.Status == BookingStatus.CancelledWithPartialPayment))).OrderByDescending(b => b.Id);
                        return ReturnBookings(paid, bookings);
                    }
                    throw new BadRequestException("Invalid booking status");
                }

                bookings = (await _bookingRepository.GetBookingsWithAllInformation(b => b.ProviderId == provider!.Id && b.Status == bookingStatus)).OrderByDescending(b => b.Id);
                return ReturnBookings(paid, bookings);
            }

            bookings = (await _bookingRepository.GetBookingsWithAllInformation(b => b.ProviderId == provider!.Id)).OrderByDescending(b => b.Id);
            return ReturnBookings(paid, bookings);
        }

        private static IEnumerable<Booking> ReturnBookings(bool? paid, IOrderedEnumerable<Booking> bookings)
        {
            if (paid != null)
            {
                return bookings.Where(b => b.Paid == (bool)paid);
            }

            return bookings;
        }

        public async Task<Booking> PostBookingAsync(Booking booking)
        {
            foreach (var service in booking.Services)
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

            booking.ClientSeen = true;
            booking.ProviderSeen = false;

            return await _bookingRepository.AddAsync(booking);
        }

        public async Task<Booking> UpdateBookingStatusAsync(Booking booking, string status)
        {
            BookingStatus bookingStatus = GetBookingStatus(booking, status);

            booking.Status = bookingStatus;
            booking.ClientSeen = false;
            booking.ProviderSeen = false;

            return await _bookingRepository.UpdateAsync(booking);
        }

        public async Task<Booking> UpdateBookingServicesAsync(Booking booking, IEnumerable<Domain.Entities.Services.BookingService> services)
        {
            var serviceList = services.ToList();
            foreach (var service in serviceList)
            {
                var dbService = await _serviceRepository.GetAsync(s => s.Id == service.ServiceId);

                if (dbService == null)
                {
                    throw new NotFoundException($"Service {service.Id} not found");
                }

                service.Service = dbService;
            }

            await _bookingServiceRepository.DeleteListAsync(booking.Services.ToList());

            booking.Services = serviceList;
            booking.ClientSeen = false;
            booking.ProviderSeen = true;

            return await _bookingRepository.UpdateAsync(booking);
        }

        public async Task<Booking> PostBookingMaterialPricesAsync(Booking booking, IEnumerable<BookingMaterialPrice> materialPrices)
        {
            await _bookingRepository.RemoveMaterialPricesAsync(booking);

            booking.MaterialPrices = materialPrices.ToList();
            booking.ClientSeen = false;
            booking.ProviderSeen = true;

            return await _bookingRepository.UpdateAsync(booking);
        }

        public async Task<Booking> AssignBookingWorkerAsync(Booking booking, int workerId)
        {
            var worker = await _providerRepository.GetAsync(p => p.Id == workerId);

            if (worker == null)
            {
                throw new NotFoundException("Worker not found");
            }

            booking.Worker = worker;

            return await _bookingRepository.UpdateAsync(booking);
        }

        public async Task<Booking> AddBookingReviewAsync(Booking booking, Review review)
        {
            if (booking.Review != null)
            {
                throw new BadRequestException("Booking already has a review");
            }

            review.BookingId = booking.Id;
            booking.Review = review;
            booking = await _bookingRepository.UpdateAsync(booking);

            return booking;
        }

        private static BookingStatus GetBookingStatus(Booking booking, string status)
        {
            if (!Enum.TryParse(status, out BookingStatus bookingStatus))
            {
                ThrowBookingStatusException();
            }
            if (bookingStatus == BookingStatus.Pending && booking.Status != BookingStatus.Pending)
            {
                ThrowBookingStatusException();
            }
            else if (bookingStatus == BookingStatus.Rejected && (booking.Status != BookingStatus.Pending || booking.Status != BookingStatus.InNegotiation))
            {
                ThrowBookingStatusException();
            }
            else if (bookingStatus == BookingStatus.Cancelled && (booking.Status == BookingStatus.Completed || bookingStatus == BookingStatus.CancelledWithPartialPayment))
            {
                ThrowBookingStatusException();
            }
            else if (bookingStatus == BookingStatus.CancelledWithPartialPayment && booking.Status != BookingStatus.Confirmed)
            {
                ThrowBookingStatusException();
            }
            else if (bookingStatus == BookingStatus.InNegotiation && (int)booking.Status > (int)bookingStatus)
            {
                ThrowBookingStatusException();
            }
            else if (bookingStatus == BookingStatus.Confirmed && (int)booking.Status > (int)bookingStatus)
            {
                ThrowBookingStatusException();
            }
            else if (bookingStatus == BookingStatus.Completed && (int)booking.Status > (int)bookingStatus)
            {
                ThrowBookingStatusException();
            }

            return bookingStatus;
        }

        private static void ThrowBookingStatusException()
        {
            throw new BadRequestException("Invalid booking status");
        }
    }
}
