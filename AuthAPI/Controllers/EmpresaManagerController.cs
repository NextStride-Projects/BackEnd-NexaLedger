using AuthAPI.Data;
using AuthAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        public async Task<IActionResult> GetEmpresas()
        {
            if (IsAdmin())
            {
                var allEmpresas = await _context.Empresas.ToListAsync();
                return Ok(allEmpresas);
            }

            var loggedInEmpresaId = GetEmpresaIdFromToken();
            var empresa = await _context.Empresas.FindAsync(loggedInEmpresaId);

            if (empresa == null)
            {
                return NotFound("Empresa not found.");
            }

            return Ok(new List<Empresa> { empresa });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmpresa(int id, Empresa updatedEmpresa)
        {
            if (!IsAdmin())
            {
                var loggedInEmpresaId = GetEmpresaIdFromToken();
                if (id != loggedInEmpresaId)
                {
                    return ForbidResultWithMessage(
                        "You do not have access to update this Empresa."
                    );
                }
            }

            var empresa = await _context.Empresas.FindAsync(id);

            if (empresa == null)
            {
                return NotFound("Empresa not found.");
            }

            if (
                !string.IsNullOrEmpty(updatedEmpresa.Nombre)
                && empresa.Nombre != updatedEmpresa.Nombre
            )
            {
                empresa.Nombre = updatedEmpresa.Nombre;
            }

            if (
                !string.IsNullOrEmpty(updatedEmpresa.Direccion)
                && empresa.Direccion != updatedEmpresa.Direccion
            )
            {
                empresa.Direccion = updatedEmpresa.Direccion;
            }

            if (
                !string.IsNullOrEmpty(updatedEmpresa.Telefono)
                && empresa.Telefono != updatedEmpresa.Telefono
            )
            {
                empresa.Telefono = updatedEmpresa.Telefono;
            }

            if (
                !string.IsNullOrEmpty(updatedEmpresa.Email)
                && empresa.Email != updatedEmpresa.Email
            )
            {
                empresa.Email = updatedEmpresa.Email;
            }

            _context.Entry(empresa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Empresa updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmpresa(int id)
        {
            if (!IsAdmin())
            {
                var loggedInEmpresaId = GetEmpresaIdFromToken();
                if (id != loggedInEmpresaId)
                {
                    return ForbidResultWithMessage(
                        "You do not have access to delete this Empresa."
                    );
                }
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
