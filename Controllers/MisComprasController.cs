using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceJuegos.Repositories;

namespace ECommerceJuegos.Controllers
{
    [Authorize]
    public class MisComprasController : Controller
    {
        private readonly IRepositorioOrden _repoOrden;

        public MisComprasController(IRepositorioOrden repoOrden)
        {
            _repoOrden = repoOrden;
        }

        public async Task<IActionResult> Index(int pagina = 1)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId) || !int.TryParse(claimId, out int usuarioId))
                return Challenge();

            int cantidadPorPagina = 10;
            var compras = await _repoOrden.ObtenerComprasPorUsuarioPaginadoAsync(usuarioId, pagina, cantidadPorPagina);
            int totalRegistros = await _repoOrden.ContarComprasPorUsuarioAsync(usuarioId);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / cantidadPorPagina);

            return View(compras);
        }
    }
}