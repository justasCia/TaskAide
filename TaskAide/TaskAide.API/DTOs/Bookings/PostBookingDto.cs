namespace TaskAide.API.DTOs.Bookings
{
    public class PostBookingDto : BookingRequestDto
    {
        public int ProviderId { get; set; }
    }
}
