using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceJuegos.Models;
using ECommerceJuegos.Repositories;

namespace ECommerceJuegos.Controllers
{
    [Authorize]
    public class ResenasController : Controller
    {
        private readonly IRepositorioResena _repoResena;
        private readonly IRepositorioOrden _repoOrden;

        public ResenasController(IRepositorioResena repoResena, IRepositorioOrden repoOrden)
        {
            _repoResena = repoResena;
            _repoOrden = repoOrden;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(int ProductoId, int Puntuacion, string Comentario)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out int usuarioId)) return Unauthorized();

            if (Puntuacion < 1 || Puntuacion > 5)
                return BadRequest("La puntuación debe estar entre 1 y 5.");

            bool compro = await _repoOrden.UsuarioComproProductoAsync(usuarioId, ProductoId);
            if (!compro)
                return Forbid("Solo puedes reseñar productos que hayas comprado.");

            var resenaExistente = await _repoResena.ObtenerPorUsuarioYProductoAsync(usuarioId, ProductoId);

            if (resenaExistente != null)
            {
                resenaExistente.Puntuacion = Puntuacion;
                resenaExistente.Comentario = Comentario;
                await _repoResena.ModificarAsync(resenaExistente);
            }
            else
            {
                var nuevaResena = new Resena
                {
                    UsuarioId = usuarioId,
                    ProductoId = ProductoId,
                    Puntuacion = Puntuacion,
                    Comentario = Comentario
                };
                await _repoResena.AgregarAsync(nuevaResena);
            }
            return RedirectToAction("Detalle", "Productos", new { id = ProductoId });
        }
    }
}