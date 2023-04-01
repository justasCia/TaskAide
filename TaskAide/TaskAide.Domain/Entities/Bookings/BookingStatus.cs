namespace TaskAide.Domain.Entities.Bookings
{
    public enum BookingStatus
    {
        Pending,
        Rejected,
        InNegotiation,
        Confirmed,
        Completed,
        Cancelled,
        CancelledWithPartialPayment
    }
}
