namespace TaskAide.Domain.Entities.Bookings
{
    public enum BookingStatus
    {
        Pending,
        Rejected,
        InNegotiation,
        Confirmed,
        Cancelled, 
        CancelledWithPartialPayment,
        Completed
    }
}
