using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Repositories;
using TaskAide.Infrastructure.Services;

namespace TaskAide.UnitTests.ServicesTests
{
    public class BookingServiceTests
    {
        private Mock<IBookingRepository> _bookingRepositoryMock;
        private Mock<IServiceRepository> _serviceRepositoryMock;
        private Mock<IProviderRepository> _providerRepositoryMock;
        private Mock<IBookingServiceRepository> _bookingServiceRepositoryMock;
        private Mock<UserManager<User>> _userManagerMock;

        private Infrastructure.Services.BookingService _bookingService;

        [SetUp]
        public void Setup()
        {
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _providerRepositoryMock = new Mock<IProviderRepository>();
            _serviceRepositoryMock = new Mock<IServiceRepository>();
            _bookingRepositoryMock = new Mock<IBookingRepository>();
            _bookingServiceRepositoryMock = new Mock<IBookingServiceRepository>();

            _bookingService = new Infrastructure.Services.BookingService(
                _bookingRepositoryMock.Object,
                _serviceRepositoryMock.Object,
                _providerRepositoryMock.Object,
                _bookingServiceRepositoryMock.Object,
                _userManagerMock.Object);
        }

        [Test]
        public async Task GetBookingAsync_WithValidId_ReturnsBooking()
        {
            var bookingId = 123;
            var expectedBooking = Builder<Booking>.CreateNew().Build();
            _bookingRepositoryMock.Setup(x => x.GetBookingsWithAllInformation(It.IsAny<Expression<Func<Booking, bool>>?>())).ReturnsAsync(new List<Booking>() { expectedBooking });

            // Act
            Booking actualBooking = await _bookingService.GetBookingAsync(bookingId);

            // Assert
            actualBooking.Should().Be(expectedBooking);
        }

        [Test]
        public async Task GetBookingAsync_WithInvalidId_ThrowsNotFoundException()
        {
            // Arrange
            int bookingId = 456;
            _bookingRepositoryMock.Setup(x => x.GetBookingsWithAllInformation(It.IsAny<Expression<Func<Booking, bool>>?>())).ReturnsAsync(new List<Booking>());

            // Act
            var act = async () => await _bookingService.GetBookingAsync(bookingId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task GetBookingsAsync_WithValidUserIdAndNoStatus_ReturnsAllUsergBookings()
        {
            // Arrange
            var user = Builder<User>.CreateNew().Build();
            var bookings = Builder<Booking>.CreateListOfSize(10)
                .Build();
            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _bookingRepositoryMock.Setup(x => x.GetBookingsWithAllInformation(It.IsAny<Expression<Func<Booking, bool>>?>())).ReturnsAsync(bookings);

            // Act
            var result = await _bookingService.GetBookingsAsync("userId", null);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(bookings.Count());
        }

        [Test]
        [TestCase(BookingStatus.Pending)]
        [TestCase(BookingStatus.Confirmed)]
        [TestCase(BookingStatus.Rejected)]
        public async Task GetBookingsAsync_WithValidUserIdAndStatus_ReturnsAllUserBookings(BookingStatus status)
        {
            // Arrange
            var user = Builder<User>.CreateNew().Build();
            var bookings = Builder<Booking>.CreateListOfSize(10)
                .All().With(b => b.Status = status)
                .Build();
            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            _bookingRepositoryMock.Setup(x => x.GetBookingsWithAllInformation(It.IsAny<Expression<Func<Booking, bool>>?>())).ReturnsAsync(bookings);

            // Act
            var result = await _bookingService.GetBookingsAsync("userId", status.ToString());

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(bookings.Count());
        }

        [Test]
        [TestCase("a")]
        [TestCase("b")]
        [TestCase("c")]
        public async Task GetBookingsAsync_WithValidUserIdAndInvalidStatus_ThrowsException(string status)
        {
            // Arrange
            var user = Builder<User>.CreateNew().Build();
            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);

            // Act
            var act = async () => await _bookingService.GetBookingsAsync("userId", status);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Test]
        public async Task GetBookingsAsync_WithInvalidUserId_ThrowsNotFoundException()
        {
            // Arrange
            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(null as User);

            // Act
            var act = async () => await _bookingService.GetBookingsAsync("userId", null);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task PostBookingAsync_WithInvalidServices_ThrowsNotFoundException()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().With(b => b.Services = Builder<Domain.Entities.Services.BookingService>.CreateListOfSize(2).Build()).Build();

            // Act
            var act = async () => await _bookingService.PostBookingAsync(booking);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task PostBookingAsync_WithInvalidProvider_ThrowsNotFoundException()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().With(b => b.Services = Builder<Domain.Entities.Services.BookingService>.CreateListOfSize(2).Build()).Build();
            _serviceRepositoryMock.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Domain.Entities.Services.Service, bool>>>())).ReturnsAsync(Builder<Domain.Entities.Services.Service>.CreateNew().Build());

            // Act
            var act = async () => await _bookingService.PostBookingAsync(booking);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task PostBookingAsync_WithInvalidUser_ThrowsNotFoundException()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().With(b => b.Services = Builder<Domain.Entities.Services.BookingService>.CreateListOfSize(2).Build()).Build();
            _serviceRepositoryMock.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Domain.Entities.Services.Service, bool>>>())).ReturnsAsync(Builder<Domain.Entities.Services.Service>.CreateNew().Build());
            _providerRepositoryMock.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>())).ReturnsAsync(Builder<Provider>.CreateNew().Build());

            // Act
            var act = async () => await _bookingService.PostBookingAsync(booking);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task PostBookingAsync_WithValidBooking_ReturnsBooking()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().With(b => b.Services = Builder<Domain.Entities.Services.BookingService>.CreateListOfSize(2).Build()).Build();
            _serviceRepositoryMock.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Domain.Entities.Services.Service, bool>>>())).ReturnsAsync(Builder<Domain.Entities.Services.Service>.CreateNew().Build());
            _providerRepositoryMock.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>())).ReturnsAsync(Builder<Provider>.CreateNew().Build());
            _userManagerMock.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(Builder<User>.CreateNew().Build());
            _bookingRepositoryMock.Setup(m => m.AddAsync(booking)).ReturnsAsync(booking);

            // Act
            var result =  await _bookingService.PostBookingAsync(booking);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(booking);
        }

        [Test]
        public async Task UpdateBookingStatusAsync_WithValidStatus_UpdatesAndReturnsBooking()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().With(x => x.Status = BookingStatus.Pending).Build();
            _bookingRepositoryMock.Setup(m => m.UpdateAsync(It.IsAny<Booking>())).ReturnsAsync(booking);

            // Act
            var result = await _bookingService.UpdateBookingStatusAsync(booking, "InNegotiation");

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(BookingStatus.InNegotiation);
        }

        [Test]
        public async Task UpdateBookingStatusAsync_WithInvalidStatusString_ThrowsBadRequestException()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().With(x => x.Status = BookingStatus.Pending).Build();

            // Act
            var act = async() => await _bookingService.UpdateBookingStatusAsync(booking, "abc");

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Test]
        [TestCase(BookingStatus.InNegotiation, BookingStatus.Pending)]
        [TestCase(BookingStatus.Rejected, BookingStatus.Pending)]
        [TestCase(BookingStatus.Confirmed, BookingStatus.Pending)]
        [TestCase(BookingStatus.Completed, BookingStatus.Pending)]
        [TestCase(BookingStatus.Cancelled, BookingStatus.Pending)]
        [TestCase(BookingStatus.CancelledWithPartialPayment, BookingStatus.Pending)]
        [TestCase(BookingStatus.Confirmed, BookingStatus.Rejected)]
        [TestCase(BookingStatus.Completed, BookingStatus.Cancelled)]
        [TestCase(BookingStatus.Rejected, BookingStatus.CancelledWithPartialPayment)]
        [TestCase(BookingStatus.Completed, BookingStatus.InNegotiation)]
        [TestCase(BookingStatus.Completed, BookingStatus.Confirmed)]
        public async Task UpdateBookingStatusAsync_WithInvalidStatus_ThrowsBadRequestException(BookingStatus currentStatus, BookingStatus updatedStatus)
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().With(x => x.Status = currentStatus).Build();

            // Act
            var act = async () => await _bookingService.UpdateBookingStatusAsync(booking, updatedStatus.ToString());

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Test]
        public async Task UpdateBookingServicesAsync_WithValidData_ShouldUpdateBookingServices()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().With(b => b.Id = 1).With(b => b.Services = Builder<Domain.Entities.Services.BookingService>.CreateListOfSize(2).Build()).Build();
            var services = Builder<Domain.Entities.Services.BookingService>.CreateListOfSize(3).All().With(s => s.BookingId = booking.Id).Build();
            _serviceRepositoryMock.Setup(m => m.GetAsync(It.IsAny<Expression<Func<Service, bool>>>())).ReturnsAsync(Builder<Service>.CreateNew().Build());
            _bookingRepositoryMock.Setup(m => m.UpdateAsync(It.IsAny<Booking>())).ReturnsAsync(booking);

            // Act
            var updatedBooking = await _bookingService.UpdateBookingServicesAsync(booking, services);

            // Assert
            updatedBooking.Should().NotBeNull();
            updatedBooking.Services.Should().BeEquivalentTo(services);
        }

        [Test]
        public async Task UpdateBookingServicesAsync_WithInvalidService_ShouldThrowNotFoundException()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().With(b => b.Id = 1).Build();
            var services = Builder<Domain.Entities.Services.BookingService>.CreateListOfSize(3).All().With(s => s.BookingId = booking.Id).Build();

            // Act
            var act = async () => await _bookingService.UpdateBookingServicesAsync(booking, services);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task PostBookingMaterialPricesAsync_ValidBookingAndMaterialPricesProvided_ShouldRemoveOldMaterialPricesAndAddNewOnes()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().With(b => b.MaterialPrices = Builder<BookingMaterialPrice>.CreateListOfSize(2).Build()).Build();
            var newMaterialPrices = Builder<BookingMaterialPrice>.CreateListOfSize(3).Build();
            _bookingRepositoryMock.Setup(m => m.UpdateAsync(It.IsAny<Booking>())).ReturnsAsync(booking);

            // Act
            var result = await _bookingService.PostBookingMaterialPricesAsync(booking, newMaterialPrices);

            // Assert
            result.MaterialPrices.Should().BeEquivalentTo(newMaterialPrices);
        }

        [Test]
        public async Task AssignBookingWorkerAsync_WhenWorkerNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().Build();
            var workerId = 1;
            _providerRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>()))
                .ReturnsAsync(null as Provider);

            // Act
            Func<Task> action = async () => await _bookingService.AssignBookingWorkerAsync(booking, workerId);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>("Worker not found");
        }

        [Test]
        public async Task AssignBookingWorkerAsync_WhenWorkerFound_ShouldUpdateBookingWorker()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().Build();
            var workerId = 1;
            var worker = Builder<Provider>.CreateNew().With(x => x.Id = workerId).Build();
            _providerRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>()))
                .ReturnsAsync(worker);
            _bookingRepositoryMock.Setup(x => x.UpdateAsync(booking)).ReturnsAsync(booking);

            // Act
            var result = await _bookingService.AssignBookingWorkerAsync(booking, workerId);

            // Assert
            result.Worker.Should().Be(worker);
            _bookingRepositoryMock.Verify(x => x.UpdateAsync(booking), Times.Once);
        }

        [Test]
        public async Task AddBookingReviewAsync_BookingAlreadyHasReview_ThrowsBadRequestException()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew()
                .With(b => b.Review, new Review())
                .Build();
            var review = Builder<Review>.CreateNew().Build();

            // Act
            Func<Task> action = async () => await _bookingService.AddBookingReviewAsync(booking, review);

            // Assert
            await action.Should().ThrowAsync<BadRequestException>()
                .WithMessage("Booking already has a review");
        }

        [Test]
        public async Task AddBookingReviewAsync_BookingDoesNotHaveReview_SetsReviewAndReturnsBooking()
        {
            // Arrange
            var booking = Builder<Booking>.CreateNew().Build();
            var review = Builder<Review>.CreateNew().Build();
            _bookingRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Booking>()))
                .ReturnsAsync(booking);

            // Act
            var result = await _bookingService.AddBookingReviewAsync(booking, review);

            // Assert
            result.Should().Be(booking);
            result.Review.Should().Be(review);
            _bookingRepositoryMock.Verify(r => r.UpdateAsync(booking), Times.Once);
        }
    }
}
