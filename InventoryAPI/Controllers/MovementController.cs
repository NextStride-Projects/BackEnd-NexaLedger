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
public class MovementController : ControllerBase
{
    private readonly InventoryContext _context;

    public MovementController(InventoryContext context)
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

    [HttpGet("{resourceId}")]
    public async Task<IActionResult> GetMovements(int resourceId)
    {
        var empresaId = GetEmpresaIdFromToken();
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.Id == resourceId && r.EmpresaId == empresaId);

        if (resource == null)
        {
            return NotFound("Resource not found or you do not have access to it.");
        }

        var movements = await _context.Movements
            .Where(m => m.ResourceId == resourceId)
            .ToListAsync();

        return Ok(movements);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMovement([FromBody] MovementDto movementDto)
    {
        var empresaId = GetEmpresaIdFromToken();
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.Id == movementDto.ResourceId && r.EmpresaId == empresaId);

        if (resource == null)
        {
            return NotFound("Resource not found or you do not have access to it.");
        }

        var movement = new Movement
        {
            ResourceId = resource.Id,
            UserId = GetUserIdFromToken(),
            Timestamp = DateTime.UtcNow,
            Type = movementDto.Type,
            Description = movementDto.Description
        };

        resource.LatestMovementDate = movement.Timestamp;

        _context.Movements.Add(movement);
        _context.Entry(resource).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMovements), new { resourceId = movement.ResourceId }, movement);
    }
}
