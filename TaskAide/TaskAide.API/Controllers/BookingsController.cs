﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using TaskAide.API.DTOs.Bookings;
using TaskAide.API.DTOs.Users;
using TaskAide.Domain.Entities.Auth;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Services;

namespace TaskAide.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingsService;
        private readonly IProvidersService _providersService;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;

        public BookingsController(IBookingService bookingsService, IProvidersService providersService, IMapper mapper, IAuthorizationService authorizationService)
        {
            _bookingsService = bookingsService;
            _providersService = providersService;
            _mapper = mapper;
            _authorizationService = authorizationService;
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

            return Ok(postedBooking);
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

            await _bookingsService.UpdateBookingStatus(id, status);

            return NoContent();
        }
    }
}
