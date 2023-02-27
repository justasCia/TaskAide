namespace TaskAide.Domain.Entities.Services
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = default!;

        public ICollection<Service> Services { get; set; } = default!;
    }
}
