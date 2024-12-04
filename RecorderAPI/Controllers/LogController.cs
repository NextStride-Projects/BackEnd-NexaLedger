using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecorderAPI.Data;
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
        public async Task<IActionResult> GetAllLogs()
        {
            // Ensure only Admin can access this endpoint
            if (!IsAdmin())
            {
                return ForbidResultWithMessage("You do not have access to this resource.");
            }

            // Fetch all logs from the database
            var logs = await _context.Logs.ToListAsync();

            return Ok(logs);
        }
    }
}
