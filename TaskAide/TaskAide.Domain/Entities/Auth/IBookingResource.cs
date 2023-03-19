namespace TaskAide.Domain.Entities.Auth
{
    public interface IBookingResource : IUserOwnedResource
    {
        public int ProviderId { get; set; }
    }
}
