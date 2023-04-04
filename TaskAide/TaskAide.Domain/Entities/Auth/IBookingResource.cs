namespace TaskAide.Domain.Entities.Auth
{
    public interface IBookingResource : IUserOwnedResource
    {
        public int Id { get; set; }
        public int ProviderId { get; set; }
        public int? WorkerId { get; set; }
    }
}
