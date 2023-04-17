using NetTopologySuite.Geometries;
using TaskAide.Domain.Entities.Auth;
using TaskAide.Domain.Entities.Services;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.Domain.Entities.Bookings
{
    public class Booking : BaseEntity, IBookingResource
    {
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;
        public int ProviderId { get; set; } = default!;
        public Provider Provider { get; set; } = default!;
        public int? WorkerId { get; set; }
        public Provider? Worker { get; set; }


        public ICollection<BookingService> Services { get; set; } = default!;
        public ICollection<BookingMaterialPrice> MaterialPrices { get; set; } = default!;
        public Point Address { get; set; } = default!;
        public string PlaceId { get; set; } = default!;
        public DateTime StartDate { get; set; } = default!;
        public DateTime EndDate { get; set; } = default!;
        public string AdditionalInformation { get; set; } = default!;
        public BookingStatus Status { get; set; }
        public bool Paid { get; set; }
        public string? CheckoutId { get; set; }

        public bool ProviderSeen { get; set; }
        public bool ClientSeen { get; set; }

        public int? ReviewId { get; set; }
        public Review? Review { get; set; }

        public decimal CalculateMaterialsCost() => MaterialPrices.Sum(mc => mc.Price);
        public decimal CalculateServicesCost() => Services.Sum(s => s.Price);
        public decimal CalculateTotalCost() => CalculateMaterialsCost() + CalculateServicesCost();
    }
}