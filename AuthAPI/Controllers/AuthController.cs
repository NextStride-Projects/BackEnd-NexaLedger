using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthAPI.Data;
using AuthAPI.Models;
using AuthAPI.Utils;
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
            // Check if the email is already in use
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

            return Ok(new
            {
                message = "Empresa registered successfully!",
                empresaId = empresa.Id
            });
        }


        [HttpPost("register/user")]
        public async Task<IActionResult> RegisterUser(Usuario usuario)
        {
            var empresa = await _context.Empresas.FindAsync(usuario.EmpresaId);
            if (empresa == null)
            {
                return NotFound("Empresa not found.");
            }

            if (!empresa.Active)
            {
                return BadRequest("Cannot register a user for an inactive Empresa.");
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

            var empresa = await _context.Empresas.FindAsync(usuario.EmpresaId);

            if (empresa == null || !empresa.Active)
            {
                return Unauthorized("Cannot log in. The associated Empresa is inactive.");
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
        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.AdminKey))
            {
                return BadRequest("Admin key is required.");
            }

            if (request.AdminKey != _configuration["Admin:Key"])
            {
                var log = new Log
                {
                    Action = "AdminLoginFailed",
                    UserId = "N/A",
                    EmpresaId = 0,
                    Timestamp = DateTime.UtcNow,
                };
                await RedisPublisher.PublishLogAsync(log);

                await RedisPublisher.PublishAsync("email-events", new
                {
                    Template = "FailedAdminLogin",
                    Recipient = "javier.cader@alumnos.uneatlantico.es",
                    Subject = "Failed Admin Login Attempt",
                    Data = new { request.Ip, LoginTime = DateTime.UtcNow }
                });

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
                Data = new { request.Ip, LoginTime = DateTime.UtcNow }
            });

            return Ok(new { Token = tokenHandler.WriteToken(token), tokenDescriptor.Expires });
        }

        [HttpPut("deactivate/empresa/{id}")]
        public async Task<IActionResult> DeactivateEmpresa(int id, [FromQuery] string motive)
        {
            var isAdmin = IsAdmin();

            var empresa = await _context.Empresas.FindAsync(id);

            if (empresa == null)
            {
                return NotFound("Empresa not found.");
            }

            if (!isAdmin)
            {
                var loggedInEmpresaId = GetEmpresaIdFromToken();
                if (id != loggedInEmpresaId)
                {
                    return ForbidResultWithMessage("You do not have access to deactivate this Empresa.");
                }
            }

            empresa.Active = false;
            _context.Entry(empresa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var log = new Log
            {
                Action = "DeactivateEmpresa",
                UserId = isAdmin ? "Admin" : GetUserIdFromToken(),
                EmpresaId = isAdmin ? 0 : empresa.Id,
                AccessedEmpresaId = id,
                Timestamp = DateTime.UtcNow,
            };
            await RedisPublisher.PublishLogAsync(log);

            string recipientEmail = isAdmin ? empresa.ResponsibleEmail : "admin@example.com";
            string subject = isAdmin
                ? "Your Empresa has been deactivated"
                : "An Empresa has been deactivated by the user";
            string template = isAdmin ? "AdminDeactivatedEmpresa" : "UserDeactivatedEmpresa";

            await RedisPublisher.PublishAsync("email-events", new
            {
                Template = template,
                Recipient = recipientEmail,
                Subject = subject,
                Data = new
                {
                    EmpresaName = empresa.FullName,
                    empresa.ResponsiblePerson,
                    Motive = motive,
                    DeactivatedBy = isAdmin ? "Admin" : empresa.ResponsiblePerson
                }
            });

            return Ok(new { message = "Empresa deactivated successfully!", empresaId = id });
        }

        [HttpPut("reactivate/empresa/{id}")]
        public async Task<IActionResult> ReactivateEmpresa(int id, [FromQuery] string motive)
        {
            var isAdmin = IsAdmin();

            var empresa = await _context.Empresas.FindAsync(id);

            if (empresa == null)
            {
                return NotFound("Empresa not found.");
            }


            if (!isAdmin)
            {
                var loggedInEmpresaId = GetEmpresaIdFromToken();
                if (id != loggedInEmpresaId)
                {
                    return ForbidResultWithMessage("You do not have access to reactivate this Empresa.");
                }
            }

            empresa.Active = true;
            _context.Entry(empresa).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            var log = new Log
            {
                Action = "ReactivateEmpresa",
                UserId = isAdmin ? "Admin" : GetUserIdFromToken(),
                EmpresaId = isAdmin ? 0 : empresa.Id,
                AccessedEmpresaId = id,
                Timestamp = DateTime.UtcNow,
            };
            await RedisPublisher.PublishLogAsync(log);

            string recipientEmail = isAdmin ? empresa.ResponsibleEmail : "admin@example.com";
            string subject = isAdmin
                ? "Your Empresa has been reactivated"
                : "An Empresa has been reactivated by the user";
            string template = isAdmin ? "AdminReactivatedEmpresa" : "UserReactivatedEmpresa";

            await RedisPublisher.PublishAsync("email-events", new
            {
                Template = template,
                Recipient = recipientEmail,
                Subject = subject,
                Data = new
                {
                    EmpresaName = empresa.FullName,
                    empresa.ResponsiblePerson,
                    Motive = motive,
                    ReactivatedBy = isAdmin ? "Admin" : empresa.ResponsiblePerson
                }
            });

            return Ok(new { message = "Empresa reactivated successfully!", empresaId = id });
        }


    }
}
