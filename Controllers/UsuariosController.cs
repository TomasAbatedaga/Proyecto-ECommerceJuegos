using System.Security.Claims;
using ECommerceJuegos.Models;
using ECommerceJuegos.Repositories;
using ECommerceJuegos.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace ECommerceJuegos.Controllers;

[Authorize]
public class UsuariosController : Controller
{
    private readonly IRepositorioUsuario _repoUsuario;
    private readonly IPasswordHasher<Usuario> _hasher;

    public UsuariosController(IRepositorioUsuario repoUsuario, IPasswordHasher<Usuario> hasher)
    {
        _repoUsuario = repoUsuario;
        _hasher = hasher;
    }

    [Authorize(Policy = "Administrador")]
    public IActionResult Index() =>
        View(_repoUsuario.ObtenerTodos().Select(Resumen).ToList());

    [Authorize(Policy = "Administrador")]
    public IActionResult Details(int id)
    {
        var usuario = _repoUsuario.ObtenerPorId(id);
        return usuario == null ? NotFound() : View(Resumen(usuario));
    }

    [Authorize(Policy = "Administrador")]
    public IActionResult Create() => View(new UsuarioCreateViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "Administrador")]
    public IActionResult Create(UsuarioCreateViewModel model)
    {
        ValidarRol(model.Rol);
        if (!ModelState.IsValid) return View(model);
        model.Email = model.Email.Trim();
        try
        {
            if (_repoUsuario.ExisteEmail(model.Email))
            {
                EmailDuplicado();
                return View(model);
            }
            CrearUsuario(model.Email, model.Contrasenia, model.Rol);
            return RedirectToAction(nameof(Index));
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            EmailDuplicado();
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "No se pudo guardar el usuario.");
        }
        return View(model);
    }

    [Authorize(Policy = "Administrador")]
    public IActionResult Edit(int id)
    {
        var usuario = _repoUsuario.ObtenerPorId(id);
        if (usuario == null) return NotFound();
        return View(new UsuarioEditViewModel
        {
            UsuarioId = usuario.UsuarioId,
            Email = usuario.Email,
            Rol = usuario.Rol,
            EstadoActivo = usuario.EstadoActivo
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Authorize(Policy = "Administrador")]
    public IActionResult Edit(int id, UsuarioEditViewModel model)
    {
        if (id != model.UsuarioId) return BadRequest();
        ValidarRol(model.Rol);
        try
        {
            var usuario = _repoUsuario.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            if (!ModelState.IsValid) return View(model);
            model.Email = model.Email.Trim();
            if (_repoUsuario.ExisteEmail(model.Email, id))
            {
                EmailDuplicado();
                return View(model);
            }
            usuario.Email = model.Email;
            usuario.Rol = model.Rol;
            usuario.EstadoActivo = model.EstadoActivo;
            _repoUsuario.Modificacion(usuario);
            return RedirectToAction(nameof(Index));
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            EmailDuplicado();
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "No se pudo guardar el usuario.");
        }
        return View(model);
    }

    [Authorize(Policy = "Administrador")]
    public IActionResult Delete(int id)
    {
        var usuario = _repoUsuario.ObtenerPorId(id);
        return usuario == null ? NotFound() : View(Resumen(usuario));
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    [Authorize(Policy = "Administrador")]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            var usuario = _repoUsuario.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            _repoUsuario.Baja(id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            TempData["Error"] = "No se pudo desactivar el usuario.";
            return RedirectToAction(nameof(Index));
        }
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Registro() => View(new RegistroViewModel());

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public IActionResult Registro(RegistroViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        model.Email = model.Email.Trim();
        try
        {
            if (_repoUsuario.ExisteEmail(model.Email))
            {
                EmailDuplicado();
                return View(model);
            }
            // Rol y estado no provienen del formulario público.
            CrearUsuario(model.Email, model.Contrasenia, "Cliente");
            return RedirectToAction(nameof(Login));
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            EmailDuplicado();
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "No se pudo registrar el usuario.");
        }
        return View(model);
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var usuario = _repoUsuario.ObtenerPorEmail(model.Email.Trim());
            if (usuario == null || !usuario.EstadoActivo || !RolValido(usuario.Rol) ||
                _hasher.VerifyHashedPassword(usuario, usuario.ContraseniaHash, model.Contrasenia)
                    == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Email o contraseña incorrectos, o usuario inactivo.");
                return View(model);
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));
            return RedirectToAction("Index", "Home");
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "No se pudo iniciar sesión. Intentá nuevamente.");
            return View(model);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private void CrearUsuario(string email, string contrasenia, string rol)
    {
        var usuario = new Usuario { Email = email, Rol = rol, EstadoActivo = true };
        usuario.ContraseniaHash = _hasher.HashPassword(usuario, contrasenia);
        _repoUsuario.Alta(usuario);
    }

    private static bool RolValido(string? rol) => rol is "Administrador" or "Cliente";

    private void ValidarRol(string? rol)
    {
        if (!RolValido(rol)) ModelState.AddModelError("Rol", "El rol no es válido.");
    }

    private void EmailDuplicado() =>
        ModelState.AddModelError("Email", "Ya existe un usuario con ese email.");

    private static UsuarioResumenViewModel Resumen(Usuario usuario) => new()
    {
        UsuarioId = usuario.UsuarioId,
        Email = usuario.Email,
        Rol = usuario.Rol,
        AvatarUrl = usuario.AvatarUrl,
        EstadoActivo = usuario.EstadoActivo
    };
}