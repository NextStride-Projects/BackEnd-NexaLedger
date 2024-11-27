using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AuthAPI.Data;
using AuthAPI.Models;
using Microsoft.EntityFrameworkCore;

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

        // Helper method to get EmpresaId from the JWT claim
        private int GetEmpresaIdFromToken()
        {
            var empresaIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmpresaId")
                                 ?? throw new UnauthorizedAccessException("EmpresaId not found in token.");
            return int.Parse(empresaIdClaim.Value);
        }

        // Helper method to create a custom 403 Forbidden response
        private IActionResult ForbidResultWithMessage(string message)
        {
            return new ObjectResult(new { error = message })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(int id)
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

        [HttpGet("empresa/{empresaId}")]
        public async Task<IActionResult> GetUsuariosByEmpresa(int empresaId)
        {
            var loggedInEmpresaId = GetEmpresaIdFromToken();

            if (empresaId != loggedInEmpresaId)
            {
                return ForbidResultWithMessage("You do not have access to this Empresa's Usuarios.");
            }

            var usuarios = await _context.Usuarios.Where(u => u.EmpresaId == empresaId).ToListAsync();
            return Ok(usuarios);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUsuario(int id, Usuario updatedUsuario)
        {
            var loggedInEmpresaId = GetEmpresaIdFromToken();

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound("Usuario not found.");
            }

            if (usuario.EmpresaId != loggedInEmpresaId)
            {
                return ForbidResultWithMessage("You do not have access to update this Usuario.");
            }

            // Dynamically update only changed fields
            if (!string.IsNullOrEmpty(updatedUsuario.Name) && usuario.Name != updatedUsuario.Name)
            {
                usuario.Name = updatedUsuario.Name;
            }

            if (!string.IsNullOrEmpty(updatedUsuario.Email) && usuario.Email != updatedUsuario.Email)
            {
                usuario.Email = updatedUsuario.Email;
            }

            if (!string.IsNullOrEmpty(updatedUsuario.Password))
            {
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(updatedUsuario.Password);
            }

            _context.Entry(usuario).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var loggedInEmpresaId = GetEmpresaIdFromToken();

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound("Usuario not found.");
            }

            if (usuario.EmpresaId != loggedInEmpresaId)
            {
                return ForbidResultWithMessage("You do not have access to delete this Usuario.");
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario deleted successfully!" });
        }
    }
}
