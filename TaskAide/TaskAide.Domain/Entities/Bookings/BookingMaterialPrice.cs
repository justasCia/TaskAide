using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAide.Domain.Entities.Bookings
{
    public class BookingMaterialPrice : BaseEntity
    {
        public string Name { get; set; } = default!;
        [Column(TypeName = "decimal(6,2)")]
        public decimal Price { get; set; }

        public int BookingId { get; set; }
        public Booking Booking { get; set; } = default!;
    }
}
