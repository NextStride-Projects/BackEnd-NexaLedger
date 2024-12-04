using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthAPI.Data;
using AuthAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AuthAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly EmpresaContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(EmpresaContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register/empresa")]
        public async Task<IActionResult> RegisterEmpresa(Empresa empresa)
        {
            if (await _context.Empresas.AnyAsync(e => e.Email == empresa.Email))
            {
                return BadRequest("Email is already in use.");
            }

            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();

            await RedisPublisher.PublishLogAsync(
                new
                {
                    Action = "PostEmpresa",
                    EmpresaId = "",
                    AccessedEmpresaId = "",
                    Timestamp = DateTime.UtcNow,
                }
            );

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

            await RedisPublisher.PublishLogAsync(
                new
                {
                    Action = "PostUser",
                    EmpresaId = "",
                    AccessedEmpresaId = "",
                    Timestamp = DateTime.UtcNow,
                }
            );

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
            var key = Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("JWT Key is not configured.")
            );
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.Name, usuario.Id.ToString()),
                        new Claim("EmpresaId", usuario.EmpresaId.ToString()),
                    }
                ),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new { Token = tokenHandler.WriteToken(token), tokenDescriptor.Expires });
        }

        [HttpPost("login/admin")]
        public IActionResult AdminLogin(string adminKey)
        {
            if (adminKey != _configuration["Admin:Key"])
            {
                return Unauthorized("Invalid admin key.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("JWT Key is not configured.")
            );
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Admin") }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new { Token = tokenHandler.WriteToken(token), tokenDescriptor.Expires });
        }
    }
}
