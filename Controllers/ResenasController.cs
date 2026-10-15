using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceJuegos.Models;
using ECommerceJuegos.Repositories;
using MySqlConnector;

namespace ECommerceJuegos.Controllers
{
    [Authorize(Roles = "Cliente")]
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
        public async Task<IActionResult> Guardar(int ProductoId, int Puntuacion, string? Comentario)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out int usuarioId) || usuarioId <= 0) return Unauthorized();

            Comentario = (Comentario ?? "").Trim();
            if (ProductoId <= 0)
                ModelState.AddModelError(nameof(ProductoId), "El producto debe ser válido.");
            if (Puntuacion < 1 || Puntuacion > 5)
                ModelState.AddModelError(nameof(Puntuacion), "La puntuación debe estar entre 1 y 5.");
            if (string.IsNullOrWhiteSpace(Comentario) || Comentario.Length > 2000)
                ModelState.AddModelError(nameof(Comentario), "El comentario es obligatorio y admite hasta 2000 caracteres.");
            if (!ModelState.IsValid)
                return BadRequest("Revisa la reseña: producto válido, puntuación entre 1 y 5 y comentario obligatorio de hasta 2000 caracteres.");

            bool compro = await _repoOrden.UsuarioComproProductoAsync(usuarioId, ProductoId);
            if (!compro)
                return BadRequest("Solo puedes reseñar productos que hayas comprado.");

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
                try
                {
                    await _repoResena.AgregarAsync(nuevaResena);
                }
                catch (MySqlException ex) when (ex.Number == 1062)
                {
                    return BadRequest("Ya tienes una reseña para este producto. Recarga el detalle para editarla.");
                }
            }
            return RedirectToAction("Detalle", "Productos", new { id = ProductoId });
        }
    }
}