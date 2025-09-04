using System;

namespace Attribute.Api.Models
{
    public class Location 
    { 
        public Guid Id { get; set; } 
        public string Code { get; set; } = default!; 
        public string Name { get; set; } = default!; 
        public Guid RegionId { get; set; } 
        public Region? Region { get; set; } 
    }
}