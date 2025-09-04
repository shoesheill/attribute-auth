using System;

namespace Attribute.Api.Models
{
    public class Permission 
    { 
        public Guid Id { get; set; } 
        public string Resource { get; set; } = default!; 
        public string Action { get; set; } = default!; 
    }

    public class RolePermission 
    { 
        public Guid RoleId { get; set; } 
        public Guid PermissionId { get; set; } 
    }
}