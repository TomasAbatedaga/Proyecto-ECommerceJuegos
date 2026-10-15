using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceJuegos.Repositories;

namespace ECommerceJuegos.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class MisComprasController : Controller
    {
        private readonly IRepositorioOrden _repoOrden;

        public MisComprasController(IRepositorioOrden repoOrden)
        {
            _repoOrden = repoOrden;
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Index(int pagina = 1)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claimId) || !int.TryParse(claimId, out int usuarioId) || usuarioId <= 0)
                return Challenge();

            int cantidadPorPagina = 10;

            int totalRegistros = await _repoOrden.ContarComprasPorUsuarioAsync(usuarioId);

            int totalPaginas = Math.Max(1, (int)Math.Ceiling((double)totalRegistros / cantidadPorPagina));
            pagina = Math.Clamp(pagina, 1, totalPaginas);
            var compras = await _repoOrden.ObtenerComprasPorUsuarioPaginadoAsync(usuarioId, pagina, cantidadPorPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(compras);
        }
    }
}