﻿namespace TaskAide.Domain.Entities.Bookings
{
    public class Review : BaseEntity
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; } = default!;
    }
}