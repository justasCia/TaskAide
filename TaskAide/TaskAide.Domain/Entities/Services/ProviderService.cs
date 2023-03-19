using TaskAide.Domain.Entities.Users;

namespace TaskAide.Domain.Entities.Services
{
    public class ProviderService : BaseEntity
    {
        public int ProviderId { get; set; }
        public Provider Provider { get; set; } = default!;

        public int ServiceId { get; set; }
        public Service Service { get; set; } = default!;
    }
}
