using System;

namespace Attribute.Api.DTOs
{
    public record LoginRequest(string Username, string Password);
    public record ResourceCreate(string Title, Guid RegionId, Guid LocationId);
    public record ResourceUpdate(string? Title);
}