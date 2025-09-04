using System;

namespace Attribute.Api.Models
{
    public class Role 
    { 
        public Guid Id { get; set; } 
        public string Name { get; set; } = default!; 
        public string? Description { get; set; } 
        public Guid? ParentRoleId { get; set; } 
    }
}