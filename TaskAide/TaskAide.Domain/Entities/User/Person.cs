namespace TaskAide.Domain.Entities.User
{
    public class Person : User
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
    }
}
