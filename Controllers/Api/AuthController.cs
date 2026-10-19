using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ECommerceJuegos.Models;
using ECommerceJuegos.Repositories;
using ECommerceJuegos.ViewModels.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerceJuegos.Controllers.Api;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IRepositorioUsuario _usuarios;
    private readonly IPasswordHasher<Usuario> _hasher;
    private readonly TokenValidationParameters _jwt;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IRepositorioUsuario usuarios, IPasswordHasher<Usuario> hasher,
        IOptionsMonitor<JwtBearerOptions> jwtOptions, ILogger<AuthController> logger)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _jwt = jwtOptions.Get(JwtBearerDefaults.AuthenticationScheme).TokenValidationParameters;
        _logger = logger;
    }

    [HttpPost("login")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public ActionResult<LoginApiResponse> Login(LoginApiRequest request)
    {
        try
        {
            var usuario = _usuarios.ObtenerPorEmail(request.Email.Trim());
            if (usuario == null || !usuario.EstadoActivo ||
                (usuario.Rol != "Administrador" && usuario.Rol != "Cliente") ||
                _hasher.VerifyHashedPassword(usuario, usuario.ContraseniaHash, request.Contrasenia)
                    == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new { mensaje = "Credenciales inválidas." });
            }

            var ahora = DateTime.UtcNow;
            var expiraEn = ahora.AddHours(2);
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };
            var token = new JwtSecurityToken(
                issuer: _jwt.ValidIssuer,
                audience: _jwt.ValidAudience,
                claims: claims,
                notBefore: ahora,
                expires: expiraEn,
                signingCredentials: new SigningCredentials(_jwt.IssuerSigningKey, SecurityAlgorithms.HmacSha256));

            return Ok(new LoginApiResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiraEn = expiraEn,
                UsuarioId = usuario.UsuarioId,
                Email = usuario.Email,
                Rol = usuario.Rol
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en login API.");
            return StatusCode(500, new { mensaje = "No se pudo iniciar sesión. Intenta nuevamente más tarde." });
        }
    }
}
