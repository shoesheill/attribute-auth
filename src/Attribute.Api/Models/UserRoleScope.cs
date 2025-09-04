using System;

namespace Attribute.Api.Models
{
    public enum ScopeType { global, region, location }

    public class UserRoleScope
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public ScopeType Scope { get; set; }
        public Guid? RegionId { get; set; }
        public Guid? LocationId { get; set; }
    }
}