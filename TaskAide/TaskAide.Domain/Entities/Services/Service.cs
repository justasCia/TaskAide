using System.ComponentModel.DataAnnotations.Schema;

namespace TaskAide.Domain.Entities.Services
{
    public class Service : BaseEntity
    {
        public string Name { get; set; } = default!;
        [Column(TypeName = "decimal(6,2)")]
        public decimal DefaultPricePerHour { get; set; } = default!;

        public int CategoryId { get; set; } = default!;
        public Category Category { get; set; } = default!;
    }
}
