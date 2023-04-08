using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using TaskAide.API.Common;
using TaskAide.API.DTOs.Bookings;
using TaskAide.API.DTOs.Reviews;
using TaskAide.API.DTOs.Users;
using TaskAide.Domain.Entities.Auth;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Services;

namespace TaskAide.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingsService;
        private readonly IProviderService _providersService;
        private readonly IPaymentService _paymentService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMapper _mapper;

        private readonly string _taskAideAppUrl;

        public BookingsController(IBookingService bookingsService, IProviderService providersService, IPaymentService paymentService, IAuthorizationService authorizationService, IMapper mapper, IConfiguration configuration)
        {
            _bookingsService = bookingsService;
            _providersService = providersService;
            _paymentService = paymentService;
            _authorizationService = authorizationService;
            _mapper = mapper;

            _taskAideAppUrl = configuration[Constants.Configuration.TaskAideAppUrl]!;
        }

        [HttpPost]
        [Route("providers")]
        public async Task<IActionResult> GetAvailableProviders(BookingRequestDto booking)
        {
            var providers = await _providersService.GetProvidersForBookingAsync(_mapper.Map<Booking>(booking));

            return Ok(providers.Select(p => _mapper.Map<ProviderDto>(p)));
        }

        [HttpPost]
        public async Task<IActionResult> PostBooking(PostBookingDto booking)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var bookingToPost = _mapper.Map<Booking>(booking);
            bookingToPost.UserId = userId!;

            var postedBooking = await _bookingsService.PostBookingAsync(bookingToPost);

            return Ok(_mapper.Map<BookingDto>(postedBooking));
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings(string? status)
        {
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            var bookings = await _bookingsService.GetBookingsAsync(userId!, status);

            return Ok(bookings.Select(b => _mapper.Map<BookingDto>(b)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var booking = await _bookingsService.GetBookingAsync(id);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, booking, PolicyNames.BookingOwner);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            return Ok(_mapper.Map<BookingDto>(booking));
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] string status)
        {
            var booking = await _bookingsService.GetBookingAsync(id);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, booking, PolicyNames.BookingOwner);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            booking = await _bookingsService.UpdateBookingStatusAsync(booking, status);

            return Ok(_mapper.Map<BookingDto>(booking));
        }

        [HttpPut("{id}/services")]
        [Authorize(Roles = Roles.Provider + ", " + Roles.Company + ", " + Roles.CompanyWorker)]
        public async Task<IActionResult> UpdateBookingServices(int id, [FromBody] IEnumerable<BookingServiceDto> services)
        {
            var booking = await _bookingsService.GetBookingAsync(id);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, booking, PolicyNames.BookingOwner);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var boookingServices = services.Select(s => _mapper.Map<BookingService>(s));

            booking = await _bookingsService.UpdateBookingServicesAsync(booking, boookingServices);

            return Ok(_mapper.Map<BookingDto>(booking));
        }

        [HttpPut("{id}/materialPrices")]
        [Authorize(Roles = Roles.Provider + ", " + Roles.Company + ", " + Roles.CompanyWorker)]
        public async Task<IActionResult> PostBookingMaterialPrices(int id, [FromBody] IEnumerable<BookingMaterialPriceDto> materialPrices)
        {
            var booking = await _bookingsService.GetBookingAsync(id);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, booking, PolicyNames.BookingOwner);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            booking = await _bookingsService.PostBookingMaterialPricesAsync(booking, materialPrices.Select(mp => _mapper.Map<BookingMaterialPrice>(mp)));

            return Ok(_mapper.Map<BookingDto>(booking));
        }

        [HttpPost("{id}/worker")]
        [Authorize(Roles = Roles.Company)]
        public async Task<IActionResult> AssignWorker(int id, int workerId)
        {
            var booking = await _bookingsService.GetBookingAsync(id);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, booking, PolicyNames.BookingOwner);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            booking = await _bookingsService.AssignBookingWorkerAsync(booking, workerId);

            return Ok(_mapper.Map<BookingDto>(booking));
        }

        [HttpGet("{id}/payment")]
        [Authorize(Roles = Roles.Client)]
        public async Task<IActionResult> StartBookingPayment(int id)
        {
            var booking = await _bookingsService.GetBookingAsync(id);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, booking, PolicyNames.BookingOwner);

            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }
            var checkoutUrl = await _paymentService.CreatePaymentForBookingAsync(booking, $"{Request.Scheme}://{Request.Host}/api/bookings/{booking.Id}/payment/status", $"{_taskAideAppUrl}/orders/{booking.Id}");

            return Ok(new { checkoutUrl });
        }

        [HttpGet("{id}/payment/status")]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessBokingPayment(int id)
        {
            var booking = await _bookingsService.GetBookingAsync(id);

            booking = await _paymentService.ProcessBookingPaymentAsync(booking);

            Response.Headers.Add("Location", $"{_taskAideAppUrl}/orders/{booking.Id}");
            return new StatusCodeResult(303);
        }

        [HttpPost("{id}/review")]
        [Authorize(Roles = Roles.Client)]
        public async Task<IActionResult> PostBookingReview(int id, ReviewDto review)
        {
            var booking = await _bookingsService.GetBookingAsync(id);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, booking, PolicyNames.BookingOwner);
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            booking = await _bookingsService.AddBookingReviewAsync(booking, _mapper.Map<Review>(review));

            return Ok(_mapper.Map<BookingDto>(booking));
        }
    }
}
