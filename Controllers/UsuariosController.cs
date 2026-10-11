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
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(IRepositorioUsuario repoUsuario, IPasswordHasher<Usuario> hasher, IWebHostEnvironment environment, ILogger<UsuariosController> logger)
    {
        _repoUsuario = repoUsuario;
        _hasher = hasher;
        _environment = environment;
        _logger = logger;
    }

    [Authorize(Policy = "Administrador")]
    public IActionResult Index(int pagina = 1)
    {
        const int tamanoPagina = 10;
        var total = _repoUsuario.ContarUsuarios();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)tamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);
        return View(new UsuariosIndexViewModel
        {
            Usuarios = _repoUsuario.ObtenerPagina(pagina, tamanoPagina).Select(Resumen).ToList(),
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = total
        });
    }

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


    [HttpGet]
    public IActionResult Perfil()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) || id <= 0)
            return Forbid();
        var usuario = _repoUsuario.ObtenerPorId(id);
        return usuario == null ? NotFound() : View(PerfilDe(usuario));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Perfil(PerfilViewModel model)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) || id <= 0)
            return Forbid();

        Usuario? usuario;
        try
        {
            usuario = _repoUsuario.ObtenerPorId(id);
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "No se pudo cargar el perfil. Intentá nuevamente.");
            return View(new PerfilViewModel());
        }
        if (usuario == null) return NotFound();
        var perfil = PerfilDe(usuario);
        var archivo = model.Avatar;
        if (archivo == null || archivo.Length == 0)
            ModelState.AddModelError(nameof(model.Avatar), "Seleccioná una imagen.");
        else if (archivo.Length > 2 * 1024 * 1024)
            ModelState.AddModelError(nameof(model.Avatar), "El avatar no puede superar 2 MB.");

        if (!ModelState.IsValid) return View(perfil);

        string? nuevaRuta = null;
        var actualizado = false;
        try
        {
            var extension = Path.GetExtension(archivo!.FileName).ToLowerInvariant();
            var tipo = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => null
            };
            if (tipo == null || !string.Equals(tipo, archivo.ContentType, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.Avatar), "Usá una imagen JPG, PNG o WebP con un tipo de contenido válido.");
                return View(perfil);
            }

            await using var entrada = archivo.OpenReadStream();
            var cabecera = new byte[12];
            var leidos = await entrada.ReadAtLeastAsync(cabecera, cabecera.Length, throwOnEndOfStream: false);
            if (!FirmaValida(extension, cabecera, leidos))
            {
                ModelState.AddModelError(nameof(model.Avatar), "El contenido del archivo no corresponde a una imagen permitida.");
                return View(perfil);
            }

            var carpeta = CarpetaAvatares();
            Directory.CreateDirectory(carpeta);
            var nombre = Guid.NewGuid().ToString("N") + extension;
            nuevaRuta = "/Uploads/Avatares/" + nombre;
            var destino = Path.Combine(carpeta, nombre);
            await using (var salida = new FileStream(destino, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await salida.WriteAsync(cabecera.AsMemory(0, leidos));
                await entrada.CopyToAsync(salida);
            }

            // La comparación evita pisar otro reemplazo simultáneo.
            if (_repoUsuario.ActualizarAvatar(id, nuevaRuta, usuario.AvatarUrl) != 1)
            {
                EliminarAvatarSeguro(nuevaRuta);
                ModelState.AddModelError("", "El perfil cambió durante la carga. Recargá la página e intentá nuevamente.");
                return View(perfil);
            }
            actualizado = true;
        }
        catch (Exception)
        {
            if (!actualizado && nuevaRuta != null) EliminarAvatarSeguro(nuevaRuta);
            ModelState.AddModelError("", "No se pudo guardar el avatar. Intentá nuevamente.");
            return View(perfil);
        }

        // El avatar anterior se elimina únicamente después de actualizar la BD.
        EliminarAvatarSeguro(usuario.AvatarUrl);
        TempData["MensajePerfil"] = "Avatar actualizado.";
        return RedirectToAction(nameof(Perfil));
    }

    private static PerfilViewModel PerfilDe(Usuario usuario) => new()
    {
        Email = usuario.Email,
        Rol = usuario.Rol,
        EstadoActivo = usuario.EstadoActivo,
        AvatarUrl = usuario.AvatarUrl
    };

    private static bool FirmaValida(string extension, byte[] bytes, int cantidad)
    {
        if (extension is ".jpg" or ".jpeg")
            return cantidad >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
        if (extension == ".png")
            return cantidad >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        return extension == ".webp" && cantidad >= 12 &&
            System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" &&
            System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP";
    }

    private string CarpetaAvatares()
    {
        var raiz = Path.GetFullPath(_environment.WebRootPath);
        var uploads = Path.Combine(raiz, "Uploads");
        var carpeta = Path.Combine(uploads, "Avatares");
        foreach (var directorio in new[] { raiz, uploads, carpeta })
        {
            if (Directory.Exists(directorio) &&
                (System.IO.File.GetAttributes(directorio) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Directorio de avatares no permitido.");
        }
        return carpeta;
    }

    private void EliminarAvatarSeguro(string? ruta)
    {
        const string prefijo = "/Uploads/Avatares/";
        if (ruta == null || !ruta.StartsWith(prefijo, StringComparison.Ordinal)) return;
        var nombre = ruta[prefijo.Length..];
        // Solo archivos con nombres generados por este módulo, sin subdirectorios.
        if (!System.Text.RegularExpressions.Regex.IsMatch(nombre,
            @"\A[0-9a-f]{32}\.(jpg|jpeg|png|webp)\z")) return;
        try
        {
            var carpeta = CarpetaAvatares();
            var archivo = Path.GetFullPath(Path.Combine(carpeta, nombre));
            if (!string.Equals(Path.GetDirectoryName(archivo), carpeta, StringComparison.OrdinalIgnoreCase))
                return;
            if (System.IO.File.Exists(archivo) &&
                (System.IO.File.GetAttributes(archivo) & FileAttributes.ReparsePoint) == 0)
                System.IO.File.Delete(archivo);
        }
        catch (Exception)
        {
            // La limpieza no debe invalidar un avatar ya guardado correctamente.
            _logger.LogWarning("No se pudo limpiar un archivo de avatar del directorio administrado.");
        }
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