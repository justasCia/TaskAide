using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskAide.Domain.Entities.Auth;
using TaskAide.Domain.Entities.Users;
using TaskAide.Domain.Repositories;

namespace TaskAide.API.Handlers.Auth
{
    public class BookingOwnerAuthorizationHandler : AuthorizationHandler<BookingOwnerRequirement, IBookingResource>
    {
        private readonly IProviderRepository _providerRepository;

        public BookingOwnerAuthorizationHandler(IProviderRepository providerRepository)
        {
            _providerRepository = providerRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BookingOwnerRequirement requirement, IBookingResource resource)
        {
            var provider = await _providerRepository.GetAsync(p => p.UserId == context.User.FindFirstValue(JwtRegisteredClaimNames.Sub));

            if (context.User.IsInRole(Roles.Admin) ||
                context.User.FindFirstValue(JwtRegisteredClaimNames.Sub) == resource.UserId ||
                (provider != null && provider.Id == resource.ProviderId))
            {
                context.Succeed(requirement);
            }
        }
    }
    public record BookingOwnerRequirement : IAuthorizationRequirement;
}
