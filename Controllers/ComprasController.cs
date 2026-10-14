using System.Security.Claims;
using ECommerceJuegos.Models;
using ECommerceJuegos.Repositories;
using ECommerceJuegos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceJuegos.Controllers;

[Authorize(Roles = "Cliente")]
public class ComprasController : Controller
{
    private readonly IRepositorioCompra _compras;
    private readonly ILogger<ComprasController> _logger;
    public ComprasController(IRepositorioCompra compras, ILogger<ComprasController> logger)
    {
        _compras = compras;
        _logger = logger;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Comprar(ComprarProductoViewModel model)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId) || usuarioId <= 0)
            return Forbid();
        if (!ModelState.IsValid) return BadRequest("Producto inválido.");

        var resultado = await _compras.ComprarAsync(usuarioId, model.ProductoId);
        if (resultado.Estado == EstadoResultadoCompra.Exito)
            return RedirectToAction(nameof(Confirmacion), new { ordenId = resultado.OrdenId });
        if (resultado.Estado == EstadoResultadoCompra.UsuarioInvalido) return Forbid();
        if (resultado.Estado == EstadoResultadoCompra.ProductoNoEncontrado)
            return NotFound(resultado.Mensaje);

        Response.StatusCode = resultado.Estado switch
        {
            EstadoResultadoCompra.ProductoInactivo or EstadoResultadoCompra.SinStock => 409,
            EstadoResultadoCompra.ErrorTemporal => 503,
            _ => 500
        };
        return View("Error", resultado);
    }

    [HttpGet]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Confirmacion(int ordenId)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId) || usuarioId <= 0)
            return Forbid();
        if (ordenId <= 0) return NotFound();
        try
        {
            var model = await _compras.ObtenerConfirmacionAsync(usuarioId, ordenId);
            return model == null ? NotFound() : View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consultando confirmación de compra.");
            return StatusCode(500, "No se pudo consultar la compra. Intentá nuevamente más tarde.");
        }
    }
}
