using TaskAide.API.DTOs.Geometry;

namespace TaskAide.API.DTOs.Users
{
    public class ProviderInformationDto
    {
        public string Description { get; set; } = default!;
        public PointDto Location { get; set; } = default!;
        public int WorkingRange { get; set; }
        public string? EmploymentNumberOrCompanyCode { get; set; }
        public decimal BasePricePerHour { get; set; }
        public IEnumerable<BaseDto> ProviderServices { get; set; } = new List<BaseDto>();
    }
}
