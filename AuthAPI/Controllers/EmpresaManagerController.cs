using System.Security.Claims;
using AuthAPI.Data;
using AuthAPI.Models;
using AuthAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmpresaManagerController : ControllerBase
    {
        private readonly EmpresaContext _context;

        public EmpresaManagerController(EmpresaContext context)
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmpresa(int id)
        {
            if (!IsAdmin())
            {
                var loggedInEmpresaId = GetEmpresaIdFromToken();
                if (id != loggedInEmpresaId)
                {
                    return ForbidResultWithMessage(
                        "You do not have access to this Empresa's data."
                    );
                }
            }

            var empresa = await _context.Empresas.FindAsync(id);

            if (empresa == null)
            {
                return NotFound("Empresa not found.");
            }

            return Ok(empresa);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmpresas(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = "id",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] string? fullName = null
        )
        {
            var query = _context.Empresas.AsQueryable();

            if (!IsAdmin())
            {
                var loggedInEmpresaId = GetEmpresaIdFromToken();
                query = query.Where(e => e.Id == loggedInEmpresaId);
            }

            // Apply filtering
            if (!string.IsNullOrEmpty(fullName))
            {
                query = query.Where(e => e.FullName.Contains(fullName));
            }

            // Apply sorting
            switch (sortBy?.ToLower())
            {
                case "id":
                    query = sortDirection?.ToLower() == "desc" ? query.OrderByDescending(e => e.Id) : query.OrderBy(e => e.Id);
                    break;
                case "fullname":
                    query = sortDirection?.ToLower() == "desc" ? query.OrderByDescending(e => e.FullName) : query.OrderBy(e => e.FullName);
                    break;
                case "category":
                    query = sortDirection?.ToLower() == "desc" ? query.OrderByDescending(e => e.Category) : query.OrderBy(e => e.Category);
                    break;
                default:
                    return BadRequest("Invalid sortBy field.");
            }

            // Total items before pagination
            var totalItems = await query.CountAsync();

            // Apply pagination
            var empresas = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var response = new
            {
                totalItems,
                totalPages,
                currentPage = page,
                pageSize,
                empresas
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmpresa(int id, Empresa updatedEmpresa)
        {
            var userId = GetUserIdFromToken();

            if (!IsAdmin())
            {
                var loggedInEmpresaId = GetEmpresaIdFromToken();
                if (id != loggedInEmpresaId)
                {
                    return ForbidResultWithMessage(
                        "You do not have access to update this Empresa."
                    );
                }

                // Create a log entry
                var log = new Log
                {
                    Action = "UpdateEmpresa",
                    UserId = userId,
                    EmpresaId = loggedInEmpresaId,
                    AccessedEmpresaId = id,
                    Timestamp = DateTime.UtcNow,
                };

                await RedisPublisher.PublishLogAsync(log);
            }

            var empresa = await _context.Empresas.FindAsync(id);

            if (empresa == null)
            {
                return NotFound("Empresa not found.");
            }

            // Update fields
            empresa.Phone = updatedEmpresa.Phone ?? empresa.Phone;
            empresa.Email = updatedEmpresa.Email ?? empresa.Email;
            empresa.FullName = updatedEmpresa.FullName ?? empresa.FullName;
            empresa.Description = updatedEmpresa.Description ?? empresa.Description;
            empresa.Alias = updatedEmpresa.Alias ?? empresa.Alias;
            empresa.Category = updatedEmpresa.Category ?? empresa.Category;
            empresa.Location = updatedEmpresa.Location ?? empresa.Location;
            empresa.Active = updatedEmpresa.Active;
            empresa.Features = updatedEmpresa.Features ?? empresa.Features;
            empresa.ResponsiblePerson = updatedEmpresa.ResponsiblePerson ?? empresa.ResponsiblePerson;
            empresa.ResponsibleEmail = updatedEmpresa.ResponsibleEmail ?? empresa.ResponsibleEmail;

            _context.Entry(empresa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Empresa updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmpresa(int id)
        {
            var userId = GetUserIdFromToken();

            if (!IsAdmin())
            {
                var loggedInEmpresaId = GetEmpresaIdFromToken();
                if (id != loggedInEmpresaId)
                {
                    return ForbidResultWithMessage(
                        "You do not have access to delete this Empresa."
                    );
                }

                // Create a log entry
                var log = new Log
                {
                    Action = "DeleteEmpresa",
                    UserId = userId,
                    EmpresaId = loggedInEmpresaId,
                    AccessedEmpresaId = id,
                    Timestamp = DateTime.UtcNow,
                };

                await RedisPublisher.PublishLogAsync(log);
            }

            var empresa = await _context.Empresas.FindAsync(id);

            if (empresa == null)
            {
                return NotFound("Empresa not found.");
            }

            _context.Empresas.Remove(empresa);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Empresa deleted successfully!" });
        }
    }
}
