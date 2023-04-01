using Stripe;
using Stripe.Checkout;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Repositories;
using TaskAide.Domain.Services;

namespace TaskAide.Infrastructure.Services
{
    public class StripePaymentService : IPaymentService
    {
        private readonly IProviderRepository _providerRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly AccountService _accountService;
        private readonly SessionService _sessionService;

        public StripePaymentService(IProviderRepository providerRepository, IBookingRepository bookingRepository)
        {
            _providerRepository = providerRepository;
            _bookingRepository = bookingRepository;
            _accountService = new AccountService();
            _sessionService = new SessionService();
        }

        public async Task<Provider> AddBankAccountAsync(Provider provider, string bankAccountNumber, string ip)
        {
            if (!string.IsNullOrEmpty(provider.AccountId))
            {
                throw new BadRequestException("Provider already has a bank cccount");
            }

            var accountCreateOptions = new AccountCreateOptions
            {
                Type = "custom",
                Country = "LT",
                BusinessType = "individual",
                Capabilities = new AccountCapabilitiesOptions
                {
                    Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                },
                ExternalAccount = new AccountBankAccountOptions()
                {
                    AccountHolderName = $"{provider.User.FirstName} {provider.User.LastName}",
                    Currency = "EUR",
                    Country = "LT",
                    AccountNumber = bankAccountNumber,
                },
                TosAcceptance = new AccountTosAcceptanceOptions
                {
                    Date = DateTime.UtcNow,
                    Ip = ip,
                },
                BusinessProfile = new AccountBusinessProfileOptions
                {
                    ProductDescription = provider.Description,
                },
                Individual = new AccountIndividualOptions
                {
                    Address = new AddressOptions
                    {
                        City = "Kaunas",
                        Country = "LT",
                        Line1 = "Address line 1",
                        PostalCode = "12345",
                    },
                    Dob = new DobOptions
                    {
                        Day = 1,
                        Month = 1,
                        Year = 2000,
                    },
                    FirstName = provider.User.FirstName,
                    LastName = provider.User.LastName,
                    Email = provider.User.Email,
                },
            };

            var account = await _accountService.CreateAsync(accountCreateOptions);
            provider.BankAccount = new string('*', bankAccountNumber.Length - 4) + bankAccountNumber.Substring(bankAccountNumber.Length - 4).PadLeft(4, '*');
            provider.AccountId = account.Id;

            return await _providerRepository.UpdateAsync(provider);
        }

        public async Task<Provider> UpdateBankAccountAsync(Provider provider, string bankAccountNumber)
        {
            if (string.IsNullOrEmpty(provider.AccountId))
            {
                throw new BadRequestException("Provider does not have an account set up");
            }

            var accountUpdateOptions = new AccountUpdateOptions
            {
                ExternalAccount = new AccountBankAccountOptions()
                {
                    AccountHolderName = $"{provider.User.FirstName} {provider.User.LastName}",
                    Currency = "EUR",
                    Country = "LT",
                    AccountNumber = bankAccountNumber,
                },
                BusinessProfile = new AccountBusinessProfileOptions
                {
                    ProductDescription = provider.Description,
                }
            };

            var account = await _accountService.UpdateAsync(provider.AccountId, accountUpdateOptions);
            provider.BankAccount = new string('*', bankAccountNumber.Length - 4) + bankAccountNumber.Substring(bankAccountNumber.Length - 4).PadLeft(4, '*');
            provider.AccountId = account.Id;

            return await _providerRepository.UpdateAsync(provider);
        }

        public async Task<string> CreatePaymentForBookingAsync(Booking booking, string successUrl, string cancelUrl)
        {
            if (booking.Status != BookingStatus.Completed && booking.Status != BookingStatus.CancelledWithPartialPayment)
            {
                throw new BadRequestException("Booking needs to be completed or cancelled with partial payment to start payment");
            }
            if (booking.Paid)
            {
                throw new BadRequestException("Booking already paid");
            }

            long longPrice = (long)(booking.CalculateTotalCost() * 100);
            var options = new SessionCreateOptions
            {
                Mode = "payment",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions { Price = "price_1Ms4FdBZbZZKguSrwk1D8gSk", Quantity = longPrice },
                },
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    TransferData = new SessionPaymentIntentDataTransferDataOptions
                    {
                        Destination = booking.Provider.AccountId,
                        Amount = longPrice
                    },
                },
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            var checkout = await _sessionService.CreateAsync(options);
            booking.CheckoutId = checkout.Id;
            await _bookingRepository.UpdateAsync(booking);

            return checkout.Url;
        }

        public async Task<Booking> ProcessBookingPaymentAsync(Booking booking)
        {
            if (string.IsNullOrEmpty(booking.CheckoutId))
            {
                throw new BadRequestException("Booking does not have a checkout session");
            }

            var checkout = await _sessionService.GetAsync(booking.CheckoutId);

            booking.Paid = checkout.PaymentStatus == "paid";

            return await _bookingRepository.UpdateAsync(booking);
        }
    }
}
