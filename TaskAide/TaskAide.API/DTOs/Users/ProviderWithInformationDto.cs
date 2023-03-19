using TaskAide.API.DTOs.Geometry;
using TaskAide.API.DTOs.Services;

namespace TaskAide.API.DTOs.Users
{
    public class ProviderWithInformationDto : ProviderDto
    {
        public PointDto Location { get; set; } = default!;
        public IEnumerable<ServiceDto> ProviderServices { get; set; } = default!;
    }
}
