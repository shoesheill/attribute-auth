using System;

namespace Attribute.Api.Models
{
    public class Region 
    { 
        public Guid Id { get; set; } 
        public string Code { get; set; } = default!; 
        public string Name { get; set; } = default!; 
    }
}