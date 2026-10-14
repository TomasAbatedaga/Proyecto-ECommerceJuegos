using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceJuegos.Models;
using ECommerceJuegos.Repositories;
using ECommerceJuegos.ViewModels;
using MySqlConnector;

namespace ECommerceJuegos.Controllers;

[Authorize(Roles = "Administrador")]
public class ClavesDigitalesController : Controller
{
    private readonly IRepositorioClaveDigital _repoClaves;
    private readonly IRepositorioProducto _repoProductos;
    private readonly ILogger<ClavesDigitalesController> _logger;

    public ClavesDigitalesController(IRepositorioClaveDigital repoClaves,
        IRepositorioProducto repoProductos, ILogger<ClavesDigitalesController> logger)
    {
        _repoClaves = repoClaves;
        _repoProductos = repoProductos;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public async Task<IActionResult> Listar(int pagina = 1)
    {
        try
        {
            const int cantidad = 20;
            var total = await _repoClaves.ContarAsync();
            var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (decimal)cantidad));
            pagina = Math.Clamp(pagina, 1, totalPaginas);
            var items = await _repoClaves.ObtenerPaginaAsync(pagina, cantidad);
            return Json(new { items, paginaActual = pagina, totalPaginas, totalRegistros = total });
        }
        catch (Exception ex)
        {
            return ErrorInesperado(ex);
        }
    }

    [HttpGet]
    public async Task<IActionResult> BuscarProductos(string? q)
    {
        q = (q ?? "").Trim();
        if (q.Length < 2) return Json(Array.Empty<ProductoBuscadorDTO>());
        try
        {
            return Json(await _repoProductos.BuscarPorNombreAsync(q));
        }
        catch (Exception ex)
        {
            return ErrorInesperado(ex);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear([FromBody] CrearClaveDigitalViewModel? model)
    {
        if (model == null) return BadRequest(new { mensaje = "Datos inválidos." });
        model.Codigo = (model.Codigo ?? "").Trim();
        ModelState.Remove(nameof(model.Codigo));
        if (!TryValidateModel(model))
            return BadRequest(new { mensaje = "Seleccioná un producto e ingresá un código de entre 1 y 255 caracteres." });

        try
        {
            // Administración de stock: se exige existencia, no estado activo.
            if (_repoProductos.ObtenerPorId(model.ProductoId) == null)
                return BadRequest(new { mensaje = "El producto seleccionado no existe." });
            if (await _repoClaves.ExisteCodigoAsync(model.Codigo))
                return Conflict(new { mensaje = "Ya existe una clave con ese código." });

            await _repoClaves.AgregarAsync(new ClaveDigital
            {
                ProductoId = model.ProductoId,
                Codigo = model.Codigo,
                Estado = "Disponible"
            });
            return Ok(new { mensaje = "Clave agregada correctamente." });
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { mensaje = "Ya existe una clave con ese código." });
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            return BadRequest(new { mensaje = "El producto seleccionado ya no existe." });
        }
        catch (Exception ex)
        {
            return ErrorInesperado(ex);
        }
    }

    [HttpPut, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar([FromBody] EditarClaveDigitalViewModel? model)
    {
        if (model == null) return BadRequest(new { mensaje = "Datos inválidos." });
        model.Codigo = (model.Codigo ?? "").Trim();
        ModelState.Remove(nameof(model.Codigo));
        if (!TryValidateModel(model))
            return BadRequest(new { mensaje = "Ingresá una clave válida y un código de entre 1 y 255 caracteres." });

        try
        {
            var existente = await _repoClaves.ObtenerPorIdAsync(model.ClaveDigitalId);
            if (existente == null) return NotFound(new { mensaje = "Clave no encontrada." });
            if (existente.Estado == "Vendida")
                return Conflict(new { mensaje = "No se puede editar una clave vendida." });
            if (existente.Codigo != model.Codigo && await _repoClaves.ExisteCodigoAsync(model.Codigo))
                return Conflict(new { mensaje = "Ya existe una clave con ese código." });

            var exito = await _repoClaves.ModificarAsync(new ClaveDigital
            {
                ClaveDigitalId = model.ClaveDigitalId,
                Codigo = model.Codigo
            });
            return exito
                ? Ok(new { mensaje = "Clave actualizada." })
                : Conflict(new { mensaje = "La clave cambió o ya no está disponible. Actualizá el listado." });
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { mensaje = "Ya existe una clave con ese código." });
        }
        catch (Exception ex)
        {
            return ErrorInesperado(ex);
        }
    }

    [HttpDelete, ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (id <= 0) return BadRequest(new { mensaje = "Identificador inválido." });
        try
        {
            var existente = await _repoClaves.ObtenerPorIdAsync(id);
            if (existente == null) return NotFound(new { mensaje = "Clave no encontrada." });
            if (existente.Estado == "Vendida")
                return Conflict(new { mensaje = "No se puede eliminar una clave vendida." });

            return await _repoClaves.EliminarAsync(id)
                ? Ok(new { mensaje = "Clave eliminada." })
                : Conflict(new { mensaje = "La clave cambió o ya no está disponible. Actualizá el listado." });
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            return Conflict(new { mensaje = "No se puede eliminar una clave asociada a un historial." });
        }
        catch (Exception ex)
        {
            return ErrorInesperado(ex);
        }
    }

    private IActionResult ErrorInesperado(Exception ex)
    {
        _logger.LogError(ex, "Error en la administración de claves digitales.");
        return StatusCode(500, new { mensaje = "No se pudo completar la operación. Intentá nuevamente." });
    }
}
