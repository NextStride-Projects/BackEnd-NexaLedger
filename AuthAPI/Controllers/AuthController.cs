using Microsoft.AspNetCore.Mvc;
using AuthAPI.Data;
using AuthAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(EmpresaContext context, IConfiguration configuration) : ControllerBase
    {
        private readonly EmpresaContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        [HttpPost("register/empresa")]
        public async Task<IActionResult> Register(Empresa empresa)
        {
            if (await _context.Empresas.AnyAsync(e => e.Email == empresa.Email))
            {
                return BadRequest("Email is already in use.");
            }

            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Empresa registered successfully!" });
        }

        [HttpPost("register/user")]
        public async Task<IActionResult> RegisterUser(Usuario usuario)
        {
            var empresa = await _context.Empresas.FindAsync(usuario.EmpresaId);
            if (empresa == null)
            {
                return NotFound("Empresa not found.");
            }

            if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
            {
                return BadRequest("Email is already in use.");
            }

            usuario.Password = BCrypt.Net.BCrypt.HashPassword(usuario.Password);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario registered successfully!" });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(password, usuario.Password))
            {
                return Unauthorized("Invalid email or password.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured."));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, usuario.Id.ToString() ?? string.Empty),
                    new Claim("EmpresaId", usuario.EmpresaId.ToString() ?? string.Empty)
                ]),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                Token = tokenHandler.WriteToken(token),
                tokenDescriptor.Expires
            });
        }
    }
}
