using System.Security.Claims;
using ECommerceJuegos.Repositories;
using ECommerceJuegos.ViewModels.Api;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceJuegos.Controllers.Api;

[ApiController]
[Route("api/mis-compras")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Cliente")]
public class MisComprasApiController : ControllerBase
{
    private readonly IRepositorioOrden _ordenes;
    private readonly ILogger<MisComprasApiController> _logger;

    public MisComprasApiController(IRepositorioOrden ordenes, ILogger<MisComprasApiController> logger)
    {
        _ordenes = ordenes;
        _logger = logger;
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Listado(int pagina = 1)
    {
        var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(claimId, out int usuarioId) || usuarioId <= 0)
            return Forbid(JwtBearerDefaults.AuthenticationScheme);

        try
        {
            const int cantidad = 10;
            int totalRegistros = await _ordenes.ContarComprasPorUsuarioAsync(usuarioId);
            int totalPaginas = Math.Max(1, (int)Math.Ceiling((double)totalRegistros / cantidad));
            pagina = Math.Clamp(pagina, 1, totalPaginas);
            var compras = await _ordenes.ObtenerComprasPorUsuarioPaginadoAsync(usuarioId, pagina, cantidad);
            var items = compras.Select(compra => new CompraApiDto
            {
                OrdenId = compra.OrdenId,
                ProductoId = compra.ProductoId,
                Fecha = compra.Fecha,
                ProductoNombre = compra.ProductoNombre,
                PrecioPagado = compra.PrecioPagado,
                CodigoClave = compra.CodigoClave,
                PortadaRuta = compra.PortadaRuta
            }).ToList();

            return Ok(new { items, paginaActual = pagina, totalPaginas, totalRegistros });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando mis compras API.");
            return StatusCode(500, new { mensaje = "No se pudieron consultar las compras." });
        }
    }
}
