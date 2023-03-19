namespace TaskAide.Domain.Entities.Services
{
    public class Service : BaseEntity
    {
        public string Name { get; set; } = default!;
        public int CategoryId { get; set; } = default!;
        public Category Category { get; set; } = default!;
    }
}
