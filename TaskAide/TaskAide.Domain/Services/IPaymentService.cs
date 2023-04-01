using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.Domain.Services
{
    public interface IPaymentService
    {
        public Task<Provider> AddBankAccountAsync(Provider provider, string bankAccountNumber, string ip);
        public Task<Provider> UpdateBankAccountAsync(Provider provider, string bankAccountNumber);
        public Task<string> CreatePaymentForBookingAsync(Booking booking, string successUrl, string cancelUrl);
        public Task<Booking> ProcessBookingPaymentAsync(Booking booking);
    }
}
