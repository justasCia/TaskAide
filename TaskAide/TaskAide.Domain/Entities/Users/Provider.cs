using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations.Schema;
using TaskAide.Domain.Entities.Bookings;
using TaskAide.Domain.Entities.Services;

namespace TaskAide.Domain.Entities.Users
{
    public class Provider : BaseEntity
    {
        public string UserId { get; set; } = default!;
        public User User { get; set; } = default!;

        public string? Description { get; set; } = default!;
        public Point? Location { get; set; } = default!;
        public string? PlaceId { get; set; } = default!;

        public string? AccountId { get; set; }
        public string? BankAccount { get; set; }

        public int WorkingRange { get; set; }
        [Column(TypeName = "decimal(6,2)")]
        public decimal BasePricePerHour { get; set; }

        public bool IsCompany { get; set; }
        public string? CompanyName { get; set; }
        public ICollection<Provider> Workers { get; set; } = default!;
        public int? CompanyId { get; set; } //company id
        public Provider? Company { get; set; }

        public ICollection<Booking> Bookings { get; set; } = default!;
        public ICollection<ProviderService> ProviderServices { get; set; } = new List<ProviderService>();
    }
}
