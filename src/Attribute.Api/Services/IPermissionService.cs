using Attribute.Api.Models;

namespace Attribute.Api.Services
{
    public interface IPermissionService
    {
        Task<bool> CanUserDoAsync(Guid userId, string resource, ResourceAction action, AppResource target);
    }
}