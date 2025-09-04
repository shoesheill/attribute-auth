using Microsoft.AspNetCore.Authorization;
using Attribute.Api.Models;

namespace Attribute.Api.Authorization
{
    public class ResourceAccessRequirement(ResourceAction action) : IAuthorizationRequirement
    {
        public ResourceAction Action { get; } = action;
    }
}