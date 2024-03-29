﻿using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskAide.Domain.Entities.Auth;
using TaskAide.Domain.Entities.Users;

namespace TaskAide.API.Handlers.Auth
{
    public class ResourceOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, IUserOwnedResource>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceOwnerRequirement requirement, IUserOwnedResource resource)
        {
            if (context.User.IsInRole(Roles.Admin) ||
                context.User.FindFirstValue(JwtRegisteredClaimNames.Sub) == resource.UserId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public record ResourceOwnerRequirement : IAuthorizationRequirement;
}
