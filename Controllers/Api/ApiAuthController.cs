using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EntrePaginas.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EntrePaginas.Controllers.Api;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class ApiAuthController : ControllerBase
{
    private readonly EntrePaginasContext _context;
    private readonly IConfiguration _configuration;
    public ApiAuthController(EntrePaginasContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpPost("token")]
    public async Task<IActionResult> Token([FromBody] TokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Usuario) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { mensaje = "Ingresá usuario/correo y contraseña." });
        var usuario = await _context.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(x => x.NombreUsuario == request.Usuario.Trim() || x.Email == request.Usuario.Trim(), cancellationToken);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.Password))
            return Unauthorized(new { mensaje = "Credenciales incorrectas." });

        var keyText = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("No está configurada la clave JWT.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
        var minutes = int.TryParse(_configuration["Jwt:Minutes"], out var configuredMinutes) ? Math.Clamp(configuredMinutes, 5, 1440) : 120;
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "EntrePaginas",
            audience: _configuration["Jwt:Audience"] ?? "EntrePaginas.Clientes",
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: credentials);
        return Ok(new
        {
            access_token = new JwtSecurityTokenHandler().WriteToken(token),
            token_type = "Bearer",
            expires_in = minutes * 60,
            usuario = new { usuario.Id, usuario.NombreUsuario, usuario.Rol }
        });
    }

    public sealed class TokenRequest
    {
        public string Usuario { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
