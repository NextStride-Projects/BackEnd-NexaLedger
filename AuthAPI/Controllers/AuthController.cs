using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthAPI.Data;
using AuthAPI.Models;
using AuthAPI.Utils; // Include the Utils namespace for RedisPublisher
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

        private string GetClientIp()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
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

            var log = new Log
            {
                Action = "RegisterEmpresa",
                UserId = "N/A",
                EmpresaId = empresa.Id,
                AccessedEmpresaId = empresa.Id,
                Timestamp = DateTime.UtcNow,
            };
            await RedisPublisher.PublishLogAsync(log);

            await RedisPublisher.PublishAsync("email-events", new
            {
                Template = "NewEmpresaRegistration",
                Recipient = empresa.Email,
                Subject = "Welcome to NexaLedger",
                Data = new { EmpresaName = empresa.FullName }
            });

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

            var log = new Log
            {
                Action = "RegisterUser",
                UserId = usuario.Id.ToString(),
                EmpresaId = usuario.EmpresaId,
                AccessedEmpresaId = usuario.EmpresaId,
                AccessedUsuarioId = usuario.Id,
                Timestamp = DateTime.UtcNow,
            };
            await RedisPublisher.PublishLogAsync(log);

            await RedisPublisher.PublishAsync("email-events", new
            {
                Template = "NewUserRegistration",
                Recipient = usuario.Email,
                Subject = "Welcome to NexaLedger",
                Data = new { UserName = usuario.Name }
            });

            return Ok(new { message = "Usuario registered successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(password, usuario.Password))
            {
                var log = new Log
                {
                    Action = "UserLoginFailed",
                    UserId = "N/A",
                    EmpresaId = 0,
                    Timestamp = DateTime.UtcNow,
                };
                await RedisPublisher.PublishLogAsync(log);

                return Unauthorized("Invalid email or password.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.")
            );
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                        new Claim("EmpresaId", usuario.EmpresaId.ToString()),
                        new Claim(ClaimTypes.Role, "User"),
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

            var logSuccess = new Log
            {
                Action = "UserLoginSuccess",
                UserId = usuario.Id.ToString(),
                EmpresaId = usuario.EmpresaId,
                Timestamp = DateTime.UtcNow,
            };
            await RedisPublisher.PublishLogAsync(logSuccess);

            await RedisPublisher.PublishAsync("email-events", new
            {
                Template = "UserLogin",
                Recipient = usuario.Email,
                Subject = "Login Notification",
                Data = new { Ip = GetClientIp(), LoginTime = DateTime.UtcNow }
            });

            return Ok(new { Token = tokenHandler.WriteToken(token), tokenDescriptor.Expires });
        }

        [HttpPost("login/admin")]
        public async Task<IActionResult> AdminLogin(string adminKey)
        {
            if (string.IsNullOrEmpty(adminKey))
            {
                return BadRequest("Admin key is required.");
            }

            if (adminKey != _configuration["Admin:Key"])
            {
                var log = new Log
                {
                    Action = "AdminLoginFailed",
                    UserId = "N/A",
                    EmpresaId = 0,
                    Timestamp = DateTime.UtcNow,
                };
                await RedisPublisher.PublishLogAsync(log);

                return Unauthorized("Clave de administrador inválida.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.")
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

            var logSuccess = new Log
            {
                Action = "AdminLoginSuccess",
                UserId = "Admin",
                EmpresaId = 0,
                Timestamp = DateTime.UtcNow,
            };
            await RedisPublisher.PublishLogAsync(logSuccess);

            await RedisPublisher.PublishAsync("email-events", new
            {
                Template = "AdminLogin",
                Recipient = "javier.cader@alumnos.uneatlantico.es",
                Subject = "Admin Login Notification",
                Data = new { Ip = GetClientIp(), LoginTime = DateTime.UtcNow }
            });

            return Ok(new { Token = tokenHandler.WriteToken(token), tokenDescriptor.Expires });
        }
    }
}
