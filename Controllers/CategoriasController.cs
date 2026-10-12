using ECommerceJuegos.Models;
using ECommerceJuegos.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
namespace ECommerceJuegos.Controllers;

[Authorize(Policy = "Administrador")]
public class CategoriasController : Controller
{
    private readonly IRepositorioCategoria _categorias;
    private readonly ILogger<CategoriasController> _logger;
    public CategoriasController(IRepositorioCategoria categorias, ILogger<CategoriasController> logger)
    { _categorias = categorias; _logger = logger; }
    [HttpGet] public IActionResult Index() => View(_categorias.ObtenerTodos());
    [HttpGet]
    public IActionResult Details(int id)
    {
        var categoria = _categorias.ObtenerPorId(id);
        return categoria == null ? NotFound() : View(categoria);
    }
    [HttpGet] public IActionResult Create() => View(new Categoria());
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Create([Bind("Nombre")] Categoria categoria) => Guardar(categoria, false);
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var categoria = _categorias.ObtenerPorId(id);
        return categoria == null ? NotFound() : View(categoria);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Edit(int id, [Bind("CategoriaId,Nombre")] Categoria categoria)
    {
        if (id != categoria.CategoriaId) return BadRequest();
        return Guardar(categoria, true);
    }
    private IActionResult Guardar(Categoria categoria, bool editar)
    {
        var vista = editar ? "Edit" : "Create";
        categoria.Nombre = (categoria.Nombre ?? "").Trim();
        if (categoria.Nombre.Length == 0) ModelState.AddModelError("Nombre", "Ingresá un nombre.");
        try
        {
            if (editar && _categorias.ObtenerPorId(categoria.CategoriaId) == null) return NotFound();
            if (!ModelState.IsValid) return View(vista, categoria);
            if (_categorias.ExisteNombre(categoria.Nombre, editar ? categoria.CategoriaId : null))
            { Duplicado(); return View(vista, categoria); }
            if (editar) _categorias.Modificacion(categoria); else _categorias.Alta(categoria);
            return RedirectToAction(nameof(Index));
        }
        catch (MySqlException ex) when (ex.Number == 1062) { Duplicado(); }
        catch (Exception ex)
        { _logger.LogError(ex, "Error guardando categoría."); ModelState.AddModelError("", "No se pudo guardar la categoría."); }
        return View(vista, categoria);
    }
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var categoria = _categorias.ObtenerPorId(id);
        return categoria == null ? NotFound() : View(categoria);
    }
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            var categoria = _categorias.ObtenerPorId(id);
            if (categoria == null) return NotFound();
            if (_categorias.TieneProductos(id))
            { ModelState.AddModelError("", "No se puede eliminar una categoría con productos asociados, incluso inactivos."); return View("Delete", categoria); }
            _categorias.Baja(id); return RedirectToAction(nameof(Index));
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        { TempData["Error"] = "No se puede eliminar la categoría porque tiene productos asociados."; }
        catch (Exception ex)
        { _logger.LogError(ex, "Error eliminando categoría."); TempData["Error"] = "No se pudo eliminar la categoría."; }
        return RedirectToAction(nameof(Index));
    }
    private void Duplicado() => ModelState.AddModelError("Nombre", "Ya existe una categoría con ese nombre.");
}
