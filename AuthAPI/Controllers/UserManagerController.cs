using AuthAPI.Data;
using AuthAPI.Models;
using AuthAPI.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioManagerController : ControllerBase
    {
        private readonly EmpresaContext _context;

        public UsuarioManagerController(EmpresaContext context)
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
        public async Task<IActionResult> GetUsuario(int id)
        {
            if (!IsAdmin())
            {
                var loggedInEmpresaId = GetEmpresaIdFromToken();
                var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

                if (usuario == null)
                {
                    return NotFound("Usuario not found.");
                }

                if (usuario.EmpresaId != loggedInEmpresaId)
                {
                    return ForbidResultWithMessage("You do not have access to this Usuario.");
                }

                return Ok(usuario);
            }

            var adminUsuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            if (adminUsuario == null)
            {
                return NotFound("Usuario not found.");
            }

            return Ok(adminUsuario);
        }

        [HttpGet("empresa/{empresaId}")]
        public async Task<IActionResult> GetUsuariosByEmpresa(int empresaId)
        {
            if (!IsAdmin())
            {
                var loggedInEmpresaId = GetEmpresaIdFromToken();

                if (empresaId != loggedInEmpresaId)
                {
                    return ForbidResultWithMessage(
                        "You do not have access to this Empresa's Usuarios."
                    );
                }

                var usuarios = await _context
                    .Usuarios.Where(u => u.EmpresaId == empresaId)
                    .ToListAsync();
                return Ok(usuarios);
            }

            var adminUsuarios = await _context
                .Usuarios.Where(u => u.EmpresaId == empresaId)
                .ToListAsync();
            return Ok(adminUsuarios);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUsuario(int id, Usuario updatedUsuario)
        {
            var userId = GetUserIdFromToken();

            if (!IsAdmin())
            {
                var loggedInEmpresaId = GetEmpresaIdFromToken();
                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    return NotFound("Usuario not found.");
                }

                if (usuario.EmpresaId != loggedInEmpresaId)
                {
                    return ForbidResultWithMessage(
                        "You do not have access to update this Usuario."
                    );
                }

                // Create a log entry
                var log = new Log
                {
                    Action = "UpdateUsuario",
                    UserId = userId,
                    EmpresaId = loggedInEmpresaId,
                    AccessedUsuarioId = id,
                    Timestamp = DateTime.UtcNow,
                };

                await RedisPublisher.PublishLogAsync(log);

                UpdateUsuarioFields(usuario, updatedUsuario);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Usuario updated successfully!" });
            }

            var adminUsuario = await _context.Usuarios.FindAsync(id);

            if (adminUsuario == null)
            {
                return NotFound("Usuario not found.");
            }

            UpdateUsuarioFields(adminUsuario, updatedUsuario);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Usuario updated successfully by admin!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var userId = GetUserIdFromToken();

            if (!IsAdmin())
            {
                var loggedInEmpresaId = GetEmpresaIdFromToken();
                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    return NotFound("Usuario not found.");
                }

                if (usuario.EmpresaId != loggedInEmpresaId)
                {
                    return ForbidResultWithMessage(
                        "You do not have access to delete this Usuario."
                    );
                }

                // Create a log entry
                var log = new Log
                {
                    Action = "DeleteUsuario",
                    UserId = userId,
                    EmpresaId = loggedInEmpresaId,
                    AccessedUsuarioId = id,
                    Timestamp = DateTime.UtcNow,
                };

                await RedisPublisher.PublishLogAsync(log);

                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Usuario deleted successfully!" });
            }

            var adminUsuario = await _context.Usuarios.FindAsync(id);

            if (adminUsuario == null)
            {
                return NotFound("Usuario not found.");
            }

            _context.Usuarios.Remove(adminUsuario);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Usuario deleted successfully by admin!" });
        }

        private void UpdateUsuarioFields(Usuario usuario, Usuario updatedUsuario)
        {
            if (!string.IsNullOrEmpty(updatedUsuario.Name) && usuario.Name != updatedUsuario.Name)
            {
                usuario.Name = updatedUsuario.Name;
            }

            if (
                !string.IsNullOrEmpty(updatedUsuario.Email)
                && usuario.Email != updatedUsuario.Email
            )
            {
                usuario.Email = updatedUsuario.Email;
            }

            if (!string.IsNullOrEmpty(updatedUsuario.Password))
            {
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(updatedUsuario.Password);
            }

            _context.Entry(usuario).State = EntityState.Modified;
        }
    }
}
