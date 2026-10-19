using ECommerceJuegos.Repositories;
using ECommerceJuegos.ViewModels;
using ECommerceJuegos.ViewModels.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceJuegos.Controllers.Api;

[ApiController]
[Route("api/productos")]
[AllowAnonymous]
public class ProductosApiController : ControllerBase
{
    private readonly IRepositorioProducto _productos;
    private readonly ILogger<ProductosApiController> _logger;

    public ProductosApiController(IRepositorioProducto productos, ILogger<ProductosApiController> logger)
    {
        _productos = productos;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Listado(int pagina = 1)
    {
        try
        {
            const int cantidad = 10;
            int totalRegistros = _productos.ContarActivos();
            int totalPaginas = Math.Max(1, (int)Math.Ceiling((double)totalRegistros / cantidad));
            pagina = Math.Clamp(pagina, 1, totalPaginas);
            var items = _productos.ObtenerPaginaActivos(pagina, cantidad).Select(Mapear).ToList();
            return Ok(new { items, paginaActual = pagina, totalPaginas, totalRegistros });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando productos API.");
            return StatusCode(500, new { mensaje = "No se pudieron consultar los productos." });
        }
    }

    [HttpGet("{id:int}")]
    public ActionResult<ProductoApiDto> Detalle(int id)
    {
        if (id <= 0) return NotFound();
        try
        {
            var producto = _productos.ObtenerPorId(id);
            if (producto == null || !producto.EstadoActivo) return NotFound();
            return Ok(Mapear(producto));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando detalle de producto API.");
            return StatusCode(500, new { mensaje = "No se pudo consultar el producto." });
        }
    }

    private static ProductoApiDto Mapear(ProductoListadoViewModel producto) => new()
    {
        ProductoId = producto.ProductoId,
        Nombre = producto.Nombre,
        Descripcion = producto.Descripcion,
        Precio = producto.Precio,
        CategoriaId = producto.CategoriaId,
        CategoriaNombre = producto.CategoriaNombre,
        PortadaRuta = producto.PortadaRuta,
        TieneStock = producto.TieneStock
    };
}
