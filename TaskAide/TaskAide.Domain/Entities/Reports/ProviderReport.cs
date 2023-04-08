namespace TaskAide.Domain.Entities.Reports
{
    public class ProviderReport
    {
        public decimal MaterialsCost { get; set; }
        public decimal ServicesRevenue { get; set; }
        public decimal TotalIncome { get; set; }
        public IDictionary<string, decimal> RevenueFromEachService { get; set; } = new Dictionary<string, decimal>();
        public int BookingRequests { get; set; }
        public int BookingRequestsCancelled { get; set; }
        public int BookingRequestsCancelledWithPartialPayment { get; set; }
        public int BookingRequestsCompleted { get; set; }
        public string FavouriteBookingRequest { get; set; } = default!;
        public IEnumerable<WorkerReport>? WorkerReports { get; set; }
    }
}
