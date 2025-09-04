using System;

namespace Attribute.Api.Models
{
    public class AppUser 
    { 
        public Guid Id { get; set; } 
        public string Username { get; set; } = default!; 
        public string PasswordHash { get; set; } = default!; 
        public string? DisplayName { get; set; } 
    }
}