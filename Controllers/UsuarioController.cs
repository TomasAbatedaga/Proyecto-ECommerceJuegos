// Controllers/UsuariosController.cs
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ECommerceJuegos.Models;
using ECommerceJuegos.Repositories;

namespace ECommerceJuegos.Controllers
{
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly IRepositorioUsuario _repoUsuario;

        public UsuariosController(IRepositorioUsuario repoUsuario)
        {
            _repoUsuario = repoUsuario;
        }

        [Authorize(Policy = "Administrador")]
        public IActionResult Index()
        {
            var usuarios = _repoUsuario.ObtenerTodos();
            return View(usuarios);
        }

        [Authorize(Policy = "Administrador")]
        public IActionResult Details(int id)
        {
            var usuario = _repoUsuario.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [Authorize(Policy = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Usuario usuario)
        {
            try
            {
                _repoUsuario.Alta(usuario);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(usuario);
            }
        }

        [Authorize(Policy = "Administrador")]
        public IActionResult Edit(int id)
        {
            var usuario = _repoUsuario.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost]
        [Authorize(Policy = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Usuario usuario)
        {
            try
            {
                usuario.UsuarioId = id;
                _repoUsuario.Modificacion(usuario);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(usuario);
            }
        }

        [Authorize(Policy = "Administrador")]
        public IActionResult Delete(int id)
        {
            var usuario = _repoUsuario.ObtenerPorId(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        [HttpPost]
        [Authorize(Policy = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id, Usuario usuario)
        {
            try
            {
                _repoUsuario.Baja(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(usuario);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string email, string contrasenia)
        {
            try
            {
                var usuario = _repoUsuario.ObtenerPorEmail(email);
                
                if (usuario == null || usuario.Contrasenia != contrasenia)
                {
                    ViewBag.Error = "Email o contrasenia incorrectos";
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View();
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Usuarios");
        }
    }
}