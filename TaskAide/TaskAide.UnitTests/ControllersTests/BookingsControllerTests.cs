using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskAide.API.Controllers;
using TaskAide.API.DTOs;
using TaskAide.API.DTOs.Bookings;
using TaskAide.API.DTOs.Reviews;
using TaskAide.API.DTOs.Users;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Exceptions;
using TaskAide.Domain.Services;

namespace TaskAide.UnitTests.ControllersTests
{
    public class BookingsControllerTests
    {
        private Mock<IBookingService> _mockBookingService;
        private Mock<IProviderService> _mockProviderService;
        private Mock<IPaymentService> _mockPaymentService;
        private Mock<IAuthorizationService> _mockAuthorizationService;

        private BookingsController _bookingsController;

        [SetUp]
        public void SetUp()
        {
            _mockBookingService = new Mock<IBookingService>();
            _mockProviderService = new Mock<IProviderService>();
            _mockAuthorizationService = new Mock<IAuthorizationService>();
            _mockPaymentService = new Mock<IPaymentService>();
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });
            var mapper = mapperConfig.CreateMapper();
            var _mockConfiguration = new Mock<IConfiguration>();

            _bookingsController = new BookingsController(_mockBookingService.Object, _mockProviderService.Object, _mockPaymentService.Object, _mockAuthorizationService.Object, mapper, _mockConfiguration.Object);

            _bookingsController.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, "testUserId")
                    }, "mock"))
                }
            };
        }

        [Test]
        public async Task GetAvailableProviders_NoProvidersFound_ReturnEmptyList()
        {
            var request = Builder<BookingRequestDto>.CreateNew().Build();
            _mockProviderService.Setup(m => m.GetProvidersForBookingAsync(It.IsAny<Booking>())).ReturnsAsync(new List<Provider>());

            var result = await _bookingsController.GetAvailableProviders(request) as OkObjectResult;
            var resultValue = result!.Value as IEnumerable<ProviderDto>;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
            resultValue.Should().BeEmpty();
        }

        [Test]
        public async Task GetAvailableProviders_3ProvidersFound_ReturnListOf3()
        {
            var request = Builder<BookingRequestDto>.CreateNew().Build();
            _mockProviderService.Setup(m => m.GetProvidersForBookingAsync(It.IsAny<Booking>())).ReturnsAsync(Builder<Provider>.CreateListOfSize(3).Build);

            var result = await _bookingsController.GetAvailableProviders(request) as OkObjectResult;
            var resultValue = result!.Value as IEnumerable<ProviderDto>;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
            resultValue!.Count().Should().Be(3);
        }

        [Test]
        public async Task PostBooking_ValidInput_returnsOkObject()
        {
            _mockBookingService.Setup(m => m.PostBookingAsync(It.IsAny<Booking>())).ReturnsAsync(Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build());

            var result = await _bookingsController.PostBooking(Builder<PostBookingDto>.CreateNew().Build()) as OkObjectResult;
            var resultValue = result!.Value as BookingDto;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task PostBooking_InvalidInput_ThrowsException()
        {
            _mockBookingService.Setup(m => m.PostBookingAsync(It.IsAny<Booking>())).ThrowsAsync(new NotFoundException(""));

            var act = async () => await _bookingsController.PostBooking(Builder<PostBookingDto>.CreateNew().Build());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task GetBookings_ValidInput_returnsOkObject()
        {
            _mockBookingService.Setup(m => m.GetBookingsAsync(It.IsAny<string>(), null, null)).ReturnsAsync(Builder<Booking>.CreateListOfSize(10).Build());

            var result = await _bookingsController.GetBookings(null, null) as OkObjectResult;
            var resultValue = result!.Value as IEnumerable<BookingDto>;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
            resultValue.Should().NotBeEmpty();
        }

        [Test]
        public async Task GetBooking_ValidUserRequestingAndBookingFound_returnsOkObject()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success());


            var result = await _bookingsController.GetBooking(It.IsAny<int>()) as OkObjectResult;
            var resultValue = result!.Value as BookingDto;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task GetBooking_BookingNotFound_ThrowsException()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ThrowsAsync(new NotFoundException(""));

            var act = async () => await _bookingsController.GetBooking(It.IsAny<int>());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task GetBooking_BookingFoundButAuthorizationUnsuccessful_returnsForbid()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Failed());


            var result = await _bookingsController.GetBooking(It.IsAny<int>()) as ForbidResult;

            result.Should().NotBeNull();
        }

        [Test]
        public async Task UpdateBookingStatus_ValidUserRequestingAndBookingFound_returnsOkObject()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success());
            _mockBookingService.Setup(m => m.UpdateBookingStatusAsync(It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(booking);


            var result = await _bookingsController.UpdateBookingStatus(It.IsAny<int>(), It.IsAny<string>()) as OkObjectResult;
            var resultValue = result!.Value as BookingDto;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task UpdateBookingStatus_BookingNotFound_ThrowsException()
        {
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ThrowsAsync(new NotFoundException(""));

            var act = async () => await _bookingsController.UpdateBookingStatus(It.IsAny<int>(), It.IsAny<string>());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task UpdateBookingStatus_BookingFoundButAuthorizationUnsuccessful_returnsForbid()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Failed());

            var result = await _bookingsController.UpdateBookingStatus(It.IsAny<int>(), It.IsAny<string>()) as ForbidResult;

            result.Should().NotBeNull();
        }

        [Test]
        public async Task UpdateBookingStatus_ValidUserRequestingAndBookingFoundButBadStatus_ThrowsException()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success());
            _mockBookingService.Setup(m => m.UpdateBookingStatusAsync(It.IsAny<Booking>(), It.IsAny<string>())).ThrowsAsync(new BadRequestException("Invalid booking status"));


            var act = async () => await _bookingsController.UpdateBookingStatus(It.IsAny<int>(), It.IsAny<string>());

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Test]
        public async Task PostBookingMaterialPrices_ValidUserRequestingAndBookingFound_returnsOkObject()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success());
            _mockBookingService.Setup(m => m.PostBookingMaterialPricesAsync(It.IsAny<Booking>(), It.IsAny<IEnumerable<BookingMaterialPrice>>())).ReturnsAsync(booking);


            var result = await _bookingsController.PostBookingMaterialPrices(It.IsAny<int>(), new List<BookingMaterialPriceDto>()) as OkObjectResult;
            var resultValue = result!.Value as BookingDto;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task PostBookingMaterialPrices_BookingNotFound_ThrowsException()
        {
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ThrowsAsync(new NotFoundException(""));

            var act = async () => await _bookingsController.PostBookingMaterialPrices(It.IsAny<int>(), new List<BookingMaterialPriceDto>());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task PostBookingMaterialPrices_BookingFoundButAuthorizationUnsuccessful_returnsForbid()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Failed());

            var result = await _bookingsController.PostBookingMaterialPrices(It.IsAny<int>(), new List<BookingMaterialPriceDto>()) as ForbidResult;

            result.Should().NotBeNull();
        }

        [Test]
        public async Task AssignWorker_ValidUserRequestingAndBookingFound_returnsOkObject()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success());
            _mockBookingService.Setup(m => m.AssignBookingWorkerAsync(It.IsAny<Booking>(), It.IsAny<int>())).ReturnsAsync(booking);


            var result = await _bookingsController.AssignWorker(It.IsAny<int>(),It.IsAny<int>()) as OkObjectResult;
            var resultValue = result!.Value as BookingDto;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task AssignWorker_BookingNotFound_ThrowsException()
        {
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ThrowsAsync(new NotFoundException(""));

            var act = async () => await _bookingsController.AssignWorker(It.IsAny<int>(), It.IsAny<int>());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task AssignWorker_BookingFoundButAuthorizationUnsuccessful_returnsForbid()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Failed());

            var result = await _bookingsController.AssignWorker(It.IsAny<int>(), It.IsAny<int>()) as ForbidResult;

            result.Should().NotBeNull();
        }

        [Test]
        public async Task AssignWorker_ValidUserRequestingAndBookingFoundButAssigmentFails_ThrowsException()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success());
            _mockBookingService.Setup(m => m.AssignBookingWorkerAsync(It.IsAny<Booking>(), It.IsAny<int>())).ThrowsAsync(new NotFoundException(""));


            var act = async () => await _bookingsController.AssignWorker(It.IsAny<int>(), It.IsAny<int>());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task StartBookingPayment_ValidUserRequestingAndBookingFound_returnsOkObject()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success());
            _mockPaymentService.Setup(m => m.CreatePaymentForBookingAsync(It.IsAny<Booking>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("abc");


            var result = await _bookingsController.StartBookingPayment(It.IsAny<int>()) as OkObjectResult;
            var resultValue = result!.Value;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
            resultValue!.GetType()!.GetProperty("checkoutUrl")!.GetValue(resultValue, null).Should().Be("abc");
        }

        [Test]
        public async Task StartBookingPayment_BookingNotFound_ThrowsException()
        {
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ThrowsAsync(new NotFoundException(""));

            var act = async () => await _bookingsController.StartBookingPayment(It.IsAny<int>());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task StartBookingPayment_BookingFoundButAuthorizationUnsuccessful_returnsForbid()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Failed());

            var result = await _bookingsController.StartBookingPayment(It.IsAny<int>()) as ForbidResult;

            result.Should().NotBeNull();
        }

        [Test]
        public async Task StartBookingPayment_ValidUserRequestingAndBookingFoundButCreatingPaymentFails_ThrowsException()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success());
            _mockPaymentService.Setup(m => m.CreatePaymentForBookingAsync(It.IsAny<Booking>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new BadRequestException(""));


            var act = async () => await _bookingsController.StartBookingPayment(It.IsAny<int>());

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Test]
        public async Task ProcessBokingPayment_BookingFound_returnsRedirect()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockPaymentService.Setup(m => m.ProcessBookingPaymentAsync(It.IsAny<Booking>())).ReturnsAsync(booking);


            var result = await _bookingsController.ProcessBokingPayment(It.IsAny<int>()) as StatusCodeResult;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(303);
        }

        [Test]
        public async Task ProcessBokingPayment_BookingNotFound_ThrowsException()
        {
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ThrowsAsync(new NotFoundException(""));

            var act = async () => await _bookingsController.ProcessBokingPayment(It.IsAny<int>());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task ProcessBokingPayment_BookingFoundButProcessingFailed_ThrowsException()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockPaymentService.Setup(m => m.ProcessBookingPaymentAsync(It.IsAny<Booking>())).ThrowsAsync(new BadRequestException(""));

            var act = async () => await _bookingsController.ProcessBokingPayment(It.IsAny<int>());

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Test]
        public async Task PostBookingReview_ValidUserRequestingAndBookingFound_returnsOkObject()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success());
            _mockBookingService.Setup(m => m.AddBookingReviewAsync(It.IsAny<Booking>(), It.IsAny<Review>())).ReturnsAsync(booking);


            var result = await _bookingsController.PostBookingReview(It.IsAny<int>(), It.IsAny<ReviewDto>()) as OkObjectResult;
            var resultValue = result!.Value as BookingDto;

            result.Should().NotBeNull();
            result.StatusCode.Should().Be(200);
            resultValue.Should().NotBeNull();
        }

        [Test]
        public async Task PostBookingReview_BookingNotFound_ThrowsException()
        {
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ThrowsAsync(new NotFoundException(""));

            var act = async () => await _bookingsController.PostBookingReview(It.IsAny<int>(), It.IsAny<ReviewDto>());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Test]
        public async Task PostBookingReview_BookingFoundButAuthorizationUnsuccessful_returnsForbid()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Failed());

            var result = await _bookingsController.PostBookingReview(It.IsAny<int>(), It.IsAny<ReviewDto>()) as ForbidResult;

            result.Should().NotBeNull();
        }

        [Test]
        public async Task PostBookingReview_ValidUserRequestingAndBookingFoundButReviewFails_ThrowsException()
        {
            var booking = Builder<Booking>.CreateNew().With(x => x.UserId = "testUserId").Build();
            _mockBookingService.Setup(m => m.GetBookingAsync(It.IsAny<int>())).ReturnsAsync(booking);
            _mockAuthorizationService.Setup(m => m.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<Booking>(), It.IsAny<string>())).ReturnsAsync(AuthorizationResult.Success());
            _mockBookingService.Setup(m => m.AddBookingReviewAsync(It.IsAny<Booking>(), It.IsAny<Review>())).ThrowsAsync(new BadRequestException(""));


            var act = async () => await _bookingsController.PostBookingReview(It.IsAny<int>(), It.IsAny<ReviewDto>());

            await act.Should().ThrowAsync<BadRequestException>();
        }
    }
}
