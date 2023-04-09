using TaskAide.API.DTOs.Reviews;

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
        public ICollection<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
        public double? Rating { 
            get
            {
                return Reviews.Count > 0 ? Reviews.Average(r => r.Rating) : null;
            }
        }
        public int ReviewCount 
        {
            get
            {
                return Reviews.Count;
            }
        }
        public int ReviewCommentsCount
        {
            get
            {
                return Reviews.Count(r => r.Comment != null);
            }
        }
    }
}
