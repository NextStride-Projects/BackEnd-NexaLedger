using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecorderAPI.Data;
using RecorderAPI.Models;
using System.Linq;
using System.Security.Claims;

namespace RecorderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LogController : ControllerBase
    {
        private readonly LogContext _context;

        public LogController(LogContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }

        private IActionResult ForbidResultWithMessage(string message)
        {
            return new ObjectResult(new { error = message })
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? sortBy = "id",
    [FromQuery] string? sortDirection = "asc",
    [FromQuery] string? userId = null,
    [FromQuery] int? empresaId = null,
    [FromQuery] string? action = null // New parameter for filtering by action
)
        {
            // Ensure only Admin can access this endpoint
            if (!IsAdmin())
            {
                return ForbidResultWithMessage("You do not have access to this resource.");
            }

            var query = _context.Logs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(log => log.UserId.Contains(userId));
            }

            if (empresaId.HasValue)
            {
                query = query.Where(log => log.EmpresaId == empresaId.Value);
            }

            if (!string.IsNullOrEmpty(action))
            {
                query = query.Where(log => log.Action == action); // Filter by action
            }

            // Apply sorting
            switch (sortBy?.ToLower())
            {
                case "id":
                    query = sortDirection?.ToLower() == "desc" ? query.OrderByDescending(l => l.Id) : query.OrderBy(l => l.Id);
                    break;
                case "timestamp":
                    query = sortDirection?.ToLower() == "desc" ? query.OrderByDescending(l => l.Timestamp) : query.OrderBy(l => l.Timestamp);
                    break;
                default:
                    return BadRequest("Invalid sortBy field.");
            }

            // Apply pagination
            var totalItems = await query.CountAsync();
            var logs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                totalItems,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                currentPage = page,
                pageSize,
                logs
            });
        }


    }
}
