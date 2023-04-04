namespace TaskAide.API.DTOs.Users
{
    public class ProviderDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; } = default!;
        public string? LastName { get; set; } = default!;
        public string? CompanyName { get; set; }
        public string Email { get; set; } = default!;
        public string Description { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public decimal WorkingRange { get; set; } = default!;
        public decimal BasePricePerHour { get; set; }
        public string? BankAccount { get; set; } = default!;
    }
}
