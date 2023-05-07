using Microsoft.Extensions.Configuration;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Repositories;
using TaskAide.Infrastructure.Services;

namespace TaskAide.UnitTests.ServicesTests
{
    public class StripePaymentsServiceTests
    {
        private Mock<IProviderRepository> _providerRepositoryMock;
        private Mock<IBookingRepository> _bookingRepositoryMock;
        private StripePaymentService _stripePaymentService;

        [SetUp]
        public void SetUp()
        {
            _providerRepositoryMock = new Mock<IProviderRepository>();
            _bookingRepositoryMock = new Mock<IBookingRepository>();
            var mockConfiguration = new Mock<IConfiguration>();
            _stripePaymentService = new StripePaymentService(_providerRepositoryMock.Object, _bookingRepositoryMock.Object, mockConfiguration.Object);
        }

        [Test]
        public async Task AddBankAccountAsync_Should_ThrowBadRequestException_When_ProviderAlreadyHasBankAccount()
        {
            // Arrange
            var provider = Builder<Provider>.CreateNew().With(p => p.AccountId = "test").Build();

            // Act
            var act = async () => await _stripePaymentService.AddBankAccountAsync(provider, "1234567890", "127.0.0.1");

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }
    }
}
