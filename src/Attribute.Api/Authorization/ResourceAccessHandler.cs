using Microsoft.AspNetCore.Authorization;
using Attribute.Api.Models;
using Attribute.Api.Services;

namespace Attribute.Api.Authorization
{
    public class ResourceAccessHandler(IPermissionService svc)
        : AuthorizationHandler<ResourceAccessRequirement, AppResource>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceAccessRequirement requirement, AppResource resource)
        {
            var userId = context.User.FindFirst("userid")?.Value;
            if (string.IsNullOrEmpty(userId)) return;

            var can = await svc.CanUserDoAsync(Guid.Parse(userId), "resource", requirement.Action, resource);
            if (can) context.Succeed(requirement);
        }
    }
}