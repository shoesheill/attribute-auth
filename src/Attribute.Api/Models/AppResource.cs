using System;

namespace Attribute.Api.Models
{
    public class AppResource
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = default!;
        public Guid OwnerId { get; set; }
        public Guid RegionId { get; set; }
        public Guid LocationId { get; set; }
        public Region? Region { get; set; }
        public Location? Location { get; set; }
    }
}