using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;
using System.Linq.Expressions;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Repositories;

namespace TaskAide.UnitTests.ServicesTests
{
    public class ProviderServiceTests
    {
        private Mock<UserManager<User>> _userManagerMock;
        private Mock<IProviderRepository> _providerRepositoryMock;
        private Mock<IServiceRepository> _serviceRepositoryMock;
        private Mock<IProviderServiceRepository> _providerServiceRepositoryMock;
        private Mock<IBookingRepository> _bookingRepositoryMock;

        private TaskAide.Infrastructure.Services.ProviderService _providerService;

        [SetUp]
        public void Setup()
        {
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _providerRepositoryMock = new Mock<IProviderRepository>();
            _serviceRepositoryMock = new Mock<IServiceRepository>();
            _providerServiceRepositoryMock = new Mock<IProviderServiceRepository>();
            _bookingRepositoryMock = new Mock<IBookingRepository>();

            _providerService = new TaskAide.Infrastructure.Services.ProviderService(
                _userManagerMock.Object,
                _providerRepositoryMock.Object,
                _serviceRepositoryMock.Object,
                _providerServiceRepositoryMock.Object,
                _bookingRepositoryMock.Object
            );
        }

        [Test]
        public async Task GetProviderAsync_ReturnsProvider_WhenProviderExists()
        {
            // Arrange
            var userId = "123";
            var provider = new Provider { UserId = userId };

            _providerRepositoryMock.Setup(x => x.GetProviderWithUserInfoAsync(userId)).ReturnsAsync(provider);

            // Act
            var result = await _providerService.GetProviderAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().Be(provider);
        }

        [Test]
        public async Task GetProviderAsync_ReturnsNull_WhenProviderDoesNotExist()
        {
            // Arrange
            var userId = "123";

            _providerRepositoryMock.Setup(x => x.GetProviderWithUserInfoAsync(userId)).ReturnsAsync(null as Provider);

            // Act
            var result = await _providerService.GetProviderAsync(userId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task UpsertProviderAsync_WhenUserNotFound_ThrowsNotFoundException()
        {
            // Arrange
            var userId = "invalidUserId";
            var provider = Builder<Provider>.CreateNew().Build();


            _userManagerMock.Setup(x => x.FindByIdAsync(userId))!.ReturnsAsync(null as User);

            // Act
            Func<Task> act = async () => await _providerService.UpsertProviderAsync(userId, provider);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("user not found");
        }

        [Test]
        public async Task UpsertProviderAsync_WhenProviderDoesNotExist_CreatesNewProvider()
        {
            // Arrange
            var userId = "validUserId";
            var user = Builder<User>.CreateNew().With(x => x.Id, userId).Build();
            var provider = Builder<Provider>.CreateNew().With(x => x.UserId, userId).Build();
            var userRoles = new List<string> { Roles.Company };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).Returns(Task.FromResult(user));
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(userRoles);
            _providerRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>())).ReturnsAsync((Provider)null!);
            _providerRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Provider>())).ReturnsAsync(provider);

            // Act
            var result = await _providerService.UpsertProviderAsync(userId, provider);

            // Assert
            result.Should().BeEquivalentTo(provider, options => options.Excluding(p => p.User));
            result!.User.Should().BeEquivalentTo(user);
            result.IsCompany.Should().BeTrue();
            _providerRepositoryMock.Verify(x => x.AddAsync(provider), Times.Once);
        }

        [Test]
        public async Task UpsertProviderAsync_WhenProviderExists_UpdatesExistingProvider()
        {
            // Arrange
            var userId = "validUserId";
            var user = Builder<User>.CreateNew().With(x => x.Id, userId).Build();
            var provider = Builder<Provider>.CreateNew().With(x => x.UserId, userId).With(x => x.Description, "old description").Build();
            var updatedProvider = Builder<Provider>.CreateNew().With(x => x.UserId, userId).With(x => x.Description, "new description").Build();
            var userRoles = new List<string> { Roles.Company };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(userRoles);
            _providerRepositoryMock.Setup(x => x.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>())).ReturnsAsync(provider);
            _providerRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Provider>())).ReturnsAsync(updatedProvider);

            // Act
            var result = await _providerService.UpsertProviderAsync(userId, updatedProvider);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(updatedProvider, options => options.Excluding(p => p.User));
            _providerRepositoryMock.Verify(x => x.UpdateAsync(provider), Times.Once);
        }

        [Test]
        public async Task GetProvidersForBookingAsync_ReturnsEmptyList_WhenNoProvidersFound()
        {
            // Arrange
            var booking = new Booking { Services = new List<BookingService> { new BookingService { ServiceId = 1 } } };
            _providerRepositoryMock.Setup(repo => repo.GetProvidersWithTheirServices(It.IsAny<Expression<Func<Provider, bool>>?>())).ReturnsAsync(new List<Provider>());

            // Act
            var providers = await _providerService.GetProvidersForBookingAsync(booking);

            // Assert
            providers.Should().BeEmpty();
            providers.Any().Should().BeFalse();
        }

        [Test]
        public async Task GetProvidersForBookingAsync_ReturnsProviders_WhenProvidersFound()
        {
            // Arrange
            var booking = new Booking
            {
                Address = new Point(1, 1) { SRID = 4326 },
                Services = new List<BookingService> { new BookingService { Service = new Service { Id = 1 } } }
            };
            var providers = new List<Provider>
        {
            new Provider
            {
                Id = 1,
                AccountId = "123",
                CompanyId = null,
                Location = new Point(1,1) { SRID = 4326 },
                WorkingRange = 5,
                ProviderServices = new List<ProviderService> { new ProviderService { Service = new Service { Id = 1} } }
            }
        };
            _providerRepositoryMock.Setup(repo => repo.GetProvidersWithTheirServices(It.IsAny<Expression<Func<Provider, bool>>?>())).ReturnsAsync(providers);

            // Act
            var result = await _providerService.GetProvidersForBookingAsync(booking);

            // Assert
            result.Should().NotBeNull();
            result.Any().Should().BeTrue();
        }

        [Test]
        public async Task GetProvidersForBookingAsync_ReturnsEmptyList_WhenProvidersOutOfRange()
        {
            // Arrange
            var booking = new Booking
            {
                Address = new Point(1, 1) { SRID = 4326 },
                Services = new List<BookingService> { new BookingService { Service = new Service { Id = 1 } } }
            };
            var providers = new List<Provider>
        {
            new Provider
            {
                Id = 1,
                AccountId = "123",
                CompanyId = null,
                Location = new Point(2,2) { SRID = 4326 },
                WorkingRange = 5,
                ProviderServices = new List<ProviderService> { new ProviderService { Service = new Service { Id = 1} } }
            },
        };
            _providerRepositoryMock.Setup(repo => repo.GetProvidersWithTheirServices(It.IsAny<Expression<Func<Provider, bool>>?>())).ReturnsAsync(providers);

            // Act
            var result = await _providerService.GetProvidersForBookingAsync(booking);

            // Assert
            result.Any().Should().BeFalse();
        }

        [Test]
        public async Task GetProvidersForBookingAsync_ReturnsEmptyList_WhenProvidersBelongsToCompany()
        {
            // Arrange
            var booking = new Booking
            {
                Address = new Point(1, 1) { SRID = 4326 },
                Services = new List<BookingService> { new BookingService { Service = new Service { Id = 1 } } }
            };
            var providers = new List<Provider>
        {
            new Provider
            {
                Id = 1,
                AccountId = "123",
                CompanyId = 3,
                Location = new Point(1,1) { SRID = 4326 },
                WorkingRange = 5,
                ProviderServices = new List<ProviderService> { new ProviderService { Service = new Service { Id = 1} } }
            },
        };
            _providerRepositoryMock.Setup(repo => repo.GetProvidersWithTheirServices(It.IsAny<Expression<Func<Provider, bool>>?>())).ReturnsAsync(providers);

            // Act
            var result = await _providerService.GetProvidersForBookingAsync(booking);

            // Assert
            result.Any().Should().BeFalse();
        }

        [Test]
        public async Task GetProvidersForBookingAsync_ReturnsEmptyList_WhenProvidersDoesntHaveaccountId()
        {
            // Arrange
            var booking = new Booking
            {
                Address = new Point(1, 1) { SRID = 4326 },
                Services = new List<BookingService> { new BookingService { Service = new Service { Id = 1 } } }
            };
            var providers = new List<Provider>
        {
            new Provider
            {
                Id = 1,
                AccountId = null,
                CompanyId = null,
                Location = new Point(1,1) { SRID = 4326 },
                WorkingRange = 5,
                ProviderServices = new List<ProviderService> { new ProviderService { Service = new Service { Id = 1} } }
            },
        };
            _providerRepositoryMock.Setup(repo => repo.GetProvidersWithTheirServices(It.IsAny<Expression<Func<Provider, bool>>?>())).ReturnsAsync(providers);

            // Act
            var result = await _providerService.GetProvidersForBookingAsync(booking);

            // Assert
            result.Any().Should().BeFalse();
        }

        [Test]
        public async Task GetProvidersForBookingAsync_ReturnsProviders_WhenProviderIsSuitableCompany()
        {
            // Arrange
            var booking = new Booking
            {
                Address = new Point(1, 1) { SRID = 4326 },
                Services = new List<BookingService> { new BookingService { Service = new Service { Id = 1 } } }
            };
            var providers = new List<Provider>
        {
            new Provider
            {
                Id = 1,
                AccountId = "123",
                CompanyId = null,
                Location = new Point(1,1) { SRID = 4326 },
                WorkingRange = 5,
                ProviderServices = new List<ProviderService> { new ProviderService { Service = new Service { Id = 1} } },
                IsCompany = true,
                Workers = new List<Provider> { new Provider { } }
            },
        };
            _providerRepositoryMock.Setup(repo => repo.GetProvidersWithTheirServices(It.IsAny<Expression<Func<Provider, bool>>?>())).ReturnsAsync(providers);

            // Act
            var result = await _providerService.GetProvidersForBookingAsync(booking);

            // Assert
            result.Any().Should().BeTrue();
        }

        [Test]
        public async Task GetCompanyWorkersAsync_WhenCompanyExists_ReturnsWorkers()
        {
            // Arrange
            var userId = "123";
            var workers = Builder<Provider>.CreateListOfSize(10).Build();
            _providerRepositoryMock.Setup(r => r.GetCompanyWithAllInfoAsync(userId))
                                  .ReturnsAsync(Builder<Provider>.CreateNew().With(x => x.Workers = workers).Build());

            // Act
            var result = await _providerService.GetCompanyWorkersAsync(userId);

            // Assert
            result.ToList().Should().BeEquivalentTo(workers.ToList());
        }

        [Test]
        public async Task GetCompanyWorkersAsync_WhenCompanyDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var userId = "123";
            _providerRepositoryMock.Setup(r => r.GetCompanyWithAllInfoAsync(userId))
                                  .ReturnsAsync(null as Provider);

            // Act & Assert
            var act = async () => await _providerService.GetCompanyWorkersAsync(userId);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task AddCompanyWorkerAsync_WhenCompanyExists_ReturnsAddedWorker()
        {
            // Arrange
            var userId = "123";
            var user = Builder<User>.CreateNew().Build();
            var company = Builder<Provider>.CreateNew().With(x => x.UserId = userId).Build();
            _providerRepositoryMock.Setup(r => r.GetCompanyWithAllInfoAsync(userId))
                .ReturnsAsync(company);
            _providerRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Provider>()))
                .ReturnsAsync(Builder<Provider>.CreateNew().Build());

            // Act
            var result = await _providerService.AddCompanyWorkerAsync(userId, user);

            // Assert
            _providerRepositoryMock.Verify(r => r.AddAsync(It.Is<Provider>(w =>
                w.User == user &&
                w.Company == company
            )), Times.Once);
            result.Should().NotBeNull();
        }

        [Test]
        public async Task AddCompanyWorkerAsync_WhenCompanyDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var userId = "123";
            var user = Builder<User>.CreateNew().Build();
            _providerRepositoryMock.Setup(r => r.GetCompanyWithAllInfoAsync(userId))
                .ReturnsAsync(null as Provider);

            // Act & Assert
            var act = async () => await _providerService.AddCompanyWorkerAsync(userId, user);

            await act.Should().ThrowAsync<NotFoundException>();
            _providerRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Provider>()), Times.Never);
        }

        [Test]
        public async Task GetProviderReportAsync_WhenProviderExists_ReturnsValidReport()
        {
            // Arrange
            var userId = "123";
            var provider = Builder<Provider>.CreateNew().With(p => p.UserId = userId).Build();
            var bookings = Builder<Booking>.CreateListOfSize(10)
                .All().With(b => b.Provider = provider)
                .All().With(b => b.MaterialPrices = Builder<BookingMaterialPrice>.CreateListOfSize(2).Build())
                .All().With(b => b.Services = Builder<BookingService>.CreateListOfSize(1).All().With(bs => bs.Service = Builder<Service>.CreateNew().Build()).Build())
                .Build();
            _providerRepositoryMock.Setup(r => r.GetAsync(p => p.UserId == userId)).ReturnsAsync(provider);
            _bookingRepositoryMock.Setup(r => r.GetBookingsWithAllInformation(It.IsAny<Expression<Func<Booking, bool>>>()))
                                   .ReturnsAsync(bookings);

            // Act
            var result = await _providerService.GetProviderReportAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.MaterialsCost.Should().Be(bookings.Where(booking => booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.CancelledWithPartialPayment).Sum(b => b.CalculateMaterialsCost()));
            result.ServicesRevenue.Should().Be(bookings.Where(booking => booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.CancelledWithPartialPayment).Sum(b => b.CalculateServicesCost()));
            result.TotalIncome.Should().Be(bookings.Where(booking => booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.CancelledWithPartialPayment).Sum(b => b.CalculateTotalCost()));
            result.BookingRequests.Should().Be(bookings.Count);
            result.BookingRequestsCancelled.Should().Be(bookings.Count(b => b.Status == BookingStatus.Cancelled));
            result.BookingRequestsCancelledWithPartialPayment.Should().Be(bookings.Count(b => b.Status == BookingStatus.CancelledWithPartialPayment));
            result.BookingRequestsCompleted.Should().Be(bookings.Count(b => b.Status == BookingStatus.Completed));;
            result.WorkerReports.Should().BeNull();
        }

        [Test]
        public async Task GetProviderReportAsync_WhenProviderDoesNotExist_ThrowsNotFoundException()
        {
            // Arrange
            var userId = "123";
            _providerRepositoryMock.Setup(r => r.GetAsync(p => p.UserId == userId)).ReturnsAsync(null as Provider);

            // Act
            var act = async () => await _providerService.GetProviderReportAsync(userId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task GetProviderReportAsync_WithNoBookings_ReturnsReportWithZeroValues()
        {
            // Arrange
            var userId = "123";
            _providerRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Expression<Func<Provider, bool>>>()))
                                    .ReturnsAsync(Builder<Provider>.CreateNew().Build());
            _bookingRepositoryMock.Setup(r => r.GetBookingsWithAllInformation(It.IsAny<Expression<Func<Booking, bool>>>()))
                                   .ReturnsAsync(new List<Booking>());

            // Act
            var result = await _providerService.GetProviderReportAsync(userId);

            // Assert
            result.MaterialsCost.Should().Be(0);
            result.ServicesRevenue.Should().Be(0);
            result.TotalIncome.Should().Be(0);
            result.RevenueFromEachService.Should().BeEmpty();
            result.BookingRequests.Should().Be(0);
            result.BookingRequestsCancelled.Should().Be(0);
            result.BookingRequestsCancelledWithPartialPayment.Should().Be(0);
            result.BookingRequestsCompleted.Should().Be(0);
            result.FavouriteBookingRequest.Should().BeNull();
            result.WorkerReports.Should().BeNull();
        }

        [Test]
        public async Task GetWorkerReportAsync_WhenCalledWithValidInputs_ReturnsWorkerReport()
        {
            // Arrange
            var userId = "user123";
            var startDate = new DateTime(2022, 01, 01);
            var endDate = new DateTime(2022, 01, 31);

            var bookings = Builder<Booking>.CreateListOfSize(3)
                .All()
                    .With(b => b.Worker = new Provider { UserId = userId })
                    .With(b => b.Services = Builder<BookingService>.CreateListOfSize(2).Build().ToList())
                    .With(b => b.MaterialPrices = Builder<BookingMaterialPrice>.CreateListOfSize(2).Build())
                    .With(b => b.Services = Builder<BookingService>.CreateListOfSize(1).All().With(bs => bs.Service = Builder<Service>.CreateNew().Build()).Build())
                .Build();

            var bookingRepositoryMock = new Mock<IBookingRepository>();
            bookingRepositoryMock.Setup(r => r.GetBookingsWithAllInformation(It.IsAny<Expression<Func<Booking, bool>>>()))
                .ReturnsAsync(bookings);


            // Act
            var workerReport = await _providerService.GetWorkerReportAsync(userId, startDate, endDate);

            // Assert
            workerReport.Should().NotBeNull();
            workerReport.ServicesRevenue.Should().Be(bookings.Where(booking => booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.CancelledWithPartialPayment).Sum(b => b.CalculateMaterialsCost()));
        }
    }
}
