using ECommerceJuegos.Models;
using ECommerceJuegos.Repositories;
using ECommerceJuegos.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
namespace ECommerceJuegos.Controllers;

[Authorize(Policy = "Administrador")]
public class ProductosController : Controller
{
    private const long MaxPortada = 5 * 1024 * 1024;
    private readonly IRepositorioProducto _productos;
    private readonly IRepositorioCategoria _categorias;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ProductosController> _logger;
    public ProductosController(IRepositorioProducto productos, IRepositorioCategoria categorias,
     IWebHostEnvironment environment, ILogger<ProductosController> logger)
    { _productos = productos; _categorias = categorias; _environment = environment; _logger = logger; }
    [AllowAnonymous, HttpGet]
    public IActionResult Catalogo(int pagina = 1) => View(Listado(pagina, true));
    [AllowAnonymous, HttpGet]
    public IActionResult Detalle(int id)
    {
        var producto = _productos.ObtenerPorId(id);
        return producto == null || !producto.EstadoActivo ? NotFound() : View(producto);
    }
    [HttpGet] public IActionResult Index(int pagina = 1) => View(Listado(pagina, false));
    private ProductosIndexViewModel Listado(int pagina, bool activos)
    {
        const int cantidad = 10;
        var total = activos ? _productos.ContarActivos() : _productos.ContarTodos();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (decimal)cantidad));
        pagina = Math.Clamp(pagina, 1, totalPaginas);
        return new ProductosIndexViewModel
        {
            Productos = activos ? _productos.ObtenerPaginaActivos(pagina, cantidad) : _productos.ObtenerPagina(pagina, cantidad),
            PaginaActual = pagina,
            TotalPaginas = totalPaginas,
            TotalRegistros = total
        };
    }
    [HttpGet]
    public IActionResult Details(int id)
    {
        var producto = _productos.ObtenerPorId(id);
        return producto == null ? NotFound() : View(producto);
    }
    [HttpGet] public IActionResult Create() => Formulario("Create", new ProductoFormViewModel());
    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024), RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public Task<IActionResult> Create(ProductoFormViewModel model) => Guardar(model, false);
    [HttpGet]
    public IActionResult Edit(int id)
    {
        var producto = _productos.ObtenerPorId(id);
        return producto == null ? NotFound() : Formulario("Edit", new ProductoFormViewModel
        {
            ProductoId = producto.ProductoId,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            Precio = producto.Precio,
            CategoriaId = producto.CategoriaId,
            EstadoActivo = producto.EstadoActivo,
            PortadaRutaActual = producto.PortadaRuta
        });
    }
    [HttpPost, ValidateAntiForgeryToken]
    [RequestSizeLimit(6 * 1024 * 1024), RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public Task<IActionResult> Edit(int id, ProductoFormViewModel model)
    {
        if (id != model.ProductoId) return Task.FromResult<IActionResult>(BadRequest());
        return Guardar(model, true);
    }
    private async Task<IActionResult> Guardar(ProductoFormViewModel model, bool editar)
    {
        var vista = editar ? "Edit" : "Create";
        string? nuevaRuta = null;
        string? anterior = null;
        var actualizado = false;
        model.Nombre = (model.Nombre ?? "").Trim();
        model.Descripcion = (model.Descripcion ?? "").Trim();
        if (model.Nombre.Length == 0) ModelState.AddModelError("Nombre", "Ingresá un nombre.");
        if (model.Descripcion.Length == 0) ModelState.AddModelError("Descripcion", "Ingresá una descripción.");
        try
        {
            if (editar)
            {
                var existente = _productos.ObtenerPorId(model.ProductoId);
                if (existente == null) return NotFound();
                anterior = existente.PortadaRuta;
            }
            else { model.ProductoId = 0; model.EstadoActivo = true; }
            model.PortadaRutaActual = anterior; // Nunca usar una ruta enviada por el navegador.
            if (_categorias.ObtenerPorId(model.CategoriaId) == null)
                ModelState.AddModelError("CategoriaId", "La categoría seleccionada no existe.");
            if (!ModelState.IsValid) return Formulario(vista, model);
            if (model.Portada != null)
            {
                nuevaRuta = await GuardarPortada(model.Portada);
                if (!ModelState.IsValid) return Formulario(vista, model);
            }
            var producto = new Producto
            {
                ProductoId = model.ProductoId,
                Nombre = model.Nombre,
                Descripcion = model.Descripcion,
                Precio = model.Precio,
                CategoriaId = model.CategoriaId,
                EstadoActivo = model.EstadoActivo,
                PortadaRuta = nuevaRuta ?? anterior
            };
            if (editar)
            {
                if (_productos.Modificacion(producto, anterior) != 1)
                {
                    EliminarPortadaSeguro(nuevaRuta);
                    ModelState.AddModelError("", "El producto cambió durante la edición. Recargá la página e intentá nuevamente.");
                    return Formulario(vista, model);
                }
            }
            else _productos.Alta(producto);
            actualizado = true;
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            EliminarPortadaSeguro(nuevaRuta);
            ModelState.AddModelError("CategoriaId", "La categoría ya no está disponible. Seleccioná otra.");
            return Formulario(vista, model);
        }
        catch (Exception ex)
        {
            if (!actualizado) EliminarPortadaSeguro(nuevaRuta);
            _logger.LogError(ex, "Error guardando producto o portada.");
            ModelState.AddModelError("", "No se pudo guardar el producto o su portada. Intentá nuevamente.");
            return Formulario(vista, model);
        }
        if (nuevaRuta != null) EliminarPortadaSeguro(anterior);
        return RedirectToAction(nameof(Index));
    }
    private IActionResult Formulario(string vista, ProductoFormViewModel model)
    {
        model.Categorias = _categorias.ObtenerTodos();
        return View(vista, model);
    }
    [HttpGet]
    public IActionResult Delete(int id)
    {
        var producto = _productos.ObtenerPorId(id);
        return producto == null ? NotFound() : View(producto);
    }
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(int id)
    {
        try
        {
            if (_productos.ObtenerPorId(id) == null) return NotFound();
            _productos.BajaLogica(id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error desactivando producto.");
            TempData["Error"] = "No se pudo desactivar el producto.";
            return RedirectToAction(nameof(Index));
        }
    }
    private async Task<string?> GuardarPortada(IFormFile archivo)
    {
        if (archivo.Length == 0 || archivo.Length > MaxPortada)
        { ModelState.AddModelError("Portada", "La portada debe contener una imagen y no superar 5 MB."); return null; }
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        var tipo = extension switch
        { ".jpg" or ".jpeg" => "image/jpeg", ".png" => "image/png", ".webp" => "image/webp", _ => null };
        if (tipo == null || !string.Equals(tipo, archivo.ContentType, StringComparison.OrdinalIgnoreCase))
        { ModelState.AddModelError("Portada", "Usá JPG, PNG o WebP con un tipo de contenido válido."); return null; }
        await using var entrada = archivo.OpenReadStream();
        var cabecera = new byte[12];
        var leidos = await entrada.ReadAtLeastAsync(cabecera, cabecera.Length, throwOnEndOfStream: false);
        if (!FirmaValida(extension, cabecera, leidos))
        { ModelState.AddModelError("Portada", "El contenido del archivo no corresponde a una imagen permitida."); return null; }
        var carpeta = CarpetaPortadas();
        Directory.CreateDirectory(carpeta);
        var nombre = Guid.NewGuid().ToString("N") + extension;
        var ruta = "/Uploads/Productos/" + nombre;
        try
        {
            await using var salida = new FileStream(Path.Combine(carpeta, nombre), FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await salida.WriteAsync(cabecera.AsMemory(0, leidos));
            // Limitar también los bytes efectivamente leídos, sin confiar solo en Length.
            long cantidad = leidos;
            var buffer = new byte[81920];
            int lectura;
            while ((lectura = await entrada.ReadAsync(buffer)) > 0)
            {
                cantidad += lectura;
                if (cantidad > MaxPortada) throw new IOException("Portada demasiado grande.");
                await salida.WriteAsync(buffer.AsMemory(0, lectura));
            }
        }
        catch { EliminarPortadaSeguro(ruta); throw; }
        return ruta;
    }
    private static bool FirmaValida(string extension, byte[] bytes, int cantidad)
    {
        if (extension is ".jpg" or ".jpeg")
            return cantidad >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
        if (extension == ".png")
            return cantidad >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        return extension == ".webp" && cantidad >= 12 &&
         System.Text.Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" &&
         System.Text.Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP";
    }
    private string CarpetaPortadas()
    {
        var raiz = Path.GetFullPath(_environment.WebRootPath);
        var uploads = Path.Combine(raiz, "Uploads");
        var carpeta = Path.Combine(uploads, "Productos");
        foreach (var directorio in new[] { raiz, uploads, carpeta })
            if (Directory.Exists(directorio) && (System.IO.File.GetAttributes(directorio) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("Directorio de portadas no permitido.");
        return carpeta;
    }
    private void EliminarPortadaSeguro(string? ruta)
    {
        const string prefijo = "/Uploads/Productos/";
        if (ruta == null || !ruta.StartsWith(prefijo, StringComparison.Ordinal)) return;
        var nombre = ruta[prefijo.Length..];
        if (!System.Text.RegularExpressions.Regex.IsMatch(nombre, @"\A[0-9a-f]{32}\.(jpg|jpeg|png|webp)\z")) return;
        try
        {
            var carpeta = CarpetaPortadas();
            var archivo = Path.GetFullPath(Path.Combine(carpeta, nombre));
            if (!string.Equals(Path.GetDirectoryName(archivo), carpeta, StringComparison.OrdinalIgnoreCase)) return;
            if (System.IO.File.Exists(archivo) && (System.IO.File.GetAttributes(archivo) & FileAttributes.ReparsePoint) == 0)
                System.IO.File.Delete(archivo);
        }
        catch (Exception) { _logger.LogWarning("No se pudo limpiar un archivo de portada del directorio administrado."); }
    }
}
