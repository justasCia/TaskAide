namespace TaskAide.Domain.Entities.User
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;

    }
}
