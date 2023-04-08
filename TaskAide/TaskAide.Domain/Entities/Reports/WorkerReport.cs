namespace TaskAide.Domain.Entities.Reports
{
    public class WorkerReport
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public decimal ServicesRevenue { get; set; }
        public IDictionary<string, decimal> RevenueFromEachService { get; set; } = new Dictionary<string, decimal>();
        public int BookingRequests { get; set; }
        public int BookingRequestsCancelled { get; set; }
        public int BookingRequestsCancelledWithPartialPayment { get; set; }
        public int BookingRequestsCompleted { get; set; }
    }
}
