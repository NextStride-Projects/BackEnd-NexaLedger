using System.Security.Claims;
using InventoryAPI.Data;
using InventoryAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResourceController : ControllerBase
{
    private readonly InventoryContext _context;

    public ResourceController(InventoryContext context)
    {
        _context = context;
    }

    private int GetEmpresaIdFromToken()
    {
        var empresaIdClaim =
            User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")
            ?? throw new UnauthorizedAccessException("EmpresaId not found in token.");
        return int.Parse(empresaIdClaim.Value);
    }

    private string GetUserIdFromToken()
    {
        var userIdClaim =
            User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("UserId not found in token.");
        return userIdClaim.Value;
    }

    // New Route: Get a specific resource by ID for the logged-in Empresa
    [HttpGet("{id}")]
    public async Task<IActionResult> GetResource(int id)
    {
        var empresaId = GetEmpresaIdFromToken();
        var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == id && r.EmpresaId == empresaId);

        if (resource == null)
        {
            return NotFound("Resource not found or you do not have access to it.");
        }

        return Ok(resource);
    }

    [HttpGet]
    public async Task<IActionResult> GetResources()
    {
        var empresaId = GetEmpresaIdFromToken();
        var resources = await _context.Resources.Where(r => r.EmpresaId == empresaId).ToListAsync();
        return Ok(resources);
    }

    [HttpPost]
    public async Task<IActionResult> CreateResource(Resource resource)
    {
        var empresaId = GetEmpresaIdFromToken();
        resource.EmpresaId = empresaId;

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetResources), new { id = resource.Id }, resource);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateResource(int id, Resource updatedResource)
    {
        var empresaId = GetEmpresaIdFromToken();
        var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == id && r.EmpresaId == empresaId);

        if (resource == null)
        {
            return NotFound("Resource not found.");
        }

        resource.Name = updatedResource.Name ?? resource.Name;
        resource.Description = updatedResource.Description ?? resource.Description;
        resource.Features = updatedResource.Features.Count > 0 ? updatedResource.Features : resource.Features;
        resource.Category = updatedResource.Category ?? resource.Category;
        resource.Available = updatedResource.Available;
        resource.SaleAvailability = updatedResource.SaleAvailability;
        resource.Price = updatedResource.Price;
        resource.Size = updatedResource.Size;
        resource.Image = updatedResource.Image ?? resource.Image;

        _context.Entry(resource).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return Ok(resource);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var empresaId = GetEmpresaIdFromToken();
        var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == id && r.EmpresaId == empresaId);

        if (resource == null)
        {
            return NotFound("Resource not found.");
        }

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
