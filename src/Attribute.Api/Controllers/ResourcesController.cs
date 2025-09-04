using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Attribute.Api.Authorization;
using Attribute.Api.Data;
using Attribute.Api.DTOs;
using Attribute.Api.Models;

namespace Attribute.Api.Controllers
{
    [ApiController]
    [Route("resources")]
    [Authorize]
    public class ResourcesController(AppDb db, IAuthorizationService authService) : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetResource(Guid id)
        {
            var res = await db.Resources.Include(r => r.Region).Include(r => r.Location).FirstOrDefaultAsync(r => r.Id == id);
            if (res == null) return NotFound();

            var result = await authService.AuthorizeAsync(User, res, new ResourceAccessRequirement(ResourceAction.Read));
            if (!result.Succeeded) return Forbid();

            return Ok(res);
        }

        [HttpPost]
        public async Task<IActionResult> CreateResource([FromBody] ResourceCreate dto)
        {
            // owner is current user
            var uid = User.FindFirstValue("userid");
            var res = new AppResource
            {
                Title = dto.Title,
                OwnerId = Guid.Parse(uid!),
                RegionId = dto.RegionId,
                LocationId = dto.LocationId
            };

            var result = await authService.AuthorizeAsync(User, res, new ResourceAccessRequirement(ResourceAction.Upsert));
            if (!result.Succeeded) return Forbid();

            db.Resources.Add(res);
            await db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetResource), new { id = res.Id }, res);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateResource(Guid id, [FromBody] ResourceUpdate dto)
        {
            var res = await db.Resources.FirstOrDefaultAsync(r => r.Id == id);
            if (res == null) return NotFound();

            // authorize update (requires ownership within scope)
            var result = await authService.AuthorizeAsync(User, res, new ResourceAccessRequirement(ResourceAction.Upsert));
            if (!result.Succeeded) return Forbid();

            res.Title = dto.Title ?? res.Title;
            await db.SaveChangesAsync();
            return Ok(res);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteResource(Guid id)
        {
            var res = await db.Resources.FirstOrDefaultAsync(r => r.Id == id);
            if (res == null) return NotFound();

            var result = await authService.AuthorizeAsync(User, res, new ResourceAccessRequirement(ResourceAction.Delete));
            if (!result.Succeeded) return Forbid();

            db.Resources.Remove(res);
            await db.SaveChangesAsync();
            return NoContent();
        }
    }
}