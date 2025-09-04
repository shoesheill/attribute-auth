using Microsoft.EntityFrameworkCore;
using Attribute.Api.Data;
using Attribute.Api.Models;

namespace Attribute.Api.Services
{
    public class PermissionService(AppDb db) : IPermissionService
    {
        public async Task<bool> CanUserDoAsync(Guid userId, string resource, ResourceAction action, AppResource target)
        {
            // Shortcut: resolve user roles + scopes
            var roleScopes = await db.UserRoleScopes.Where(x => x.UserId == userId).ToListAsync();
            if (!roleScopes.Any()) return false;

            // Is NepalHead? (inherits all) -> check by role name
            var nepalHeadRoleId = await db.Roles.Where(r => r.Name == "NepalHead").Select(r => r.Id).FirstOrDefaultAsync();
            if (nepalHeadRoleId != Guid.Empty && roleScopes.Any(rs => rs.RoleId == nepalHeadRoleId && rs.Scope == ScopeType.global))
                return true;

            // Resolve if user has the (resource, action) permission via any role (ignoring scope for now)
            var actionStr = action.ToString().ToLower(); // "read", "upsert", "delete"
            var hasPermission = await (from urs in db.UserRoleScopes
                                   join rp in db.RolePermissions on urs.RoleId equals rp.RoleId
                                   join p in db.Permissions on rp.PermissionId equals p.Id
                                   where urs.UserId == userId && p.Resource == resource && p.Action.ToLower() == actionStr
                                   select 1).AnyAsync();
            if (!hasPermission) return false;

            // Scope checks (view within region/location; upsert/delete only own)
            var inRegion = roleScopes.Any(s => s.Scope == ScopeType.region && s.RegionId == target.RegionId);
            var inLocation = roleScopes.Any(s => s.Scope == ScopeType.location && s.LocationId == target.LocationId);

            if (action == ResourceAction.Read)
            {
                return inRegion || inLocation; // can view all within your scoped region/location
            }

            if (action is ResourceAction.Upsert or ResourceAction.Delete)
            {
                var owns = target.OwnerId == userId;
                return owns && (inRegion || inLocation);
            }

            return false;
        }
    }
}