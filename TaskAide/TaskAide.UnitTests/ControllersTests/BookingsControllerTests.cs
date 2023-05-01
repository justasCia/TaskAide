using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskAide.API.Controllers;
using TaskAide.API.DTOs;
using TaskAide.API.DTOs.Bookings;
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
    }
}
