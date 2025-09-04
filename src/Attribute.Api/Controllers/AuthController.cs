using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Attribute.Api.Data;
using Attribute.Api.DTOs;
using Attribute.Api.Services;

namespace Attribute.Api.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController(AppDb db, IConfiguration configuration) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user == null || user.PasswordHash != req.Password) return Unauthorized();

            var claims = new List<Claim> { new Claim("userid", user.Id.ToString()), new Claim(ClaimTypes.Name, user.Username) };
            var token = JwtService.IssueToken(claims, configuration["Jwt:Key"] ?? "super-secret-key-please-change-this-key-now");

            return Ok(new { accessToken = token });
        }
    }
}