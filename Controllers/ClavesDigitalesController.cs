using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECommerceJuegos.Models;
using ECommerceJuegos.Repositories;

namespace ECommerceJuegos.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ClavesDigitalesController : Controller
    {
        private readonly IRepositorioClaveDigital _repoClaves;
        private readonly IRepositorioProducto _repoProductos;

        public ClavesDigitalesController(IRepositorioClaveDigital repoClaves, IRepositorioProducto repoProductos)
        {
            _repoClaves = repoClaves;
            _repoProductos = repoProductos;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Listar()
        {
            var claves = await _repoClaves.ObtenerTodasAsync();
            return Json(claves);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarProductos(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Json(new List<ProductoBuscadorDTO>());

            var resultados = await _repoProductos.BuscarPorNombreAsync(q);
            return Json(resultados);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] ClaveDigital clave)
        {
            if (clave.ProductoId <= 0 || string.IsNullOrWhiteSpace(clave.Codigo))
                return BadRequest("Datos incompletos.");

            if (await _repoClaves.ExisteCodigoAsync(clave.Codigo))
                return BadRequest("El código ya existe.");

            await _repoClaves.AgregarAsync(clave);
            return Ok(new { mensaje = "Clave agregada correctamente." });
        }

        [HttpPut]
        public async Task<IActionResult> Editar([FromBody] ClaveDigital clave)
        {
            if (clave.ClaveDigitalId <= 0 || string.IsNullOrWhiteSpace(clave.Codigo))
                return BadRequest("Datos incompletos.");

            var claveExistente = await _repoClaves.ObtenerPorIdAsync(clave.ClaveDigitalId);
            if (claveExistente == null) return NotFound("Clave no encontrada.");
            
            if (claveExistente.Estado == "Vendida")
                return BadRequest("No se puede editar una clave vendida.");

            if (claveExistente.Codigo != clave.Codigo && await _repoClaves.ExisteCodigoAsync(clave.Codigo))
                return BadRequest("El nuevo código ya está en uso.");

            var exito = await _repoClaves.ModificarAsync(clave);
            if (exito) return Ok(new { mensaje = "Clave actualizada." });
            
            return BadRequest("Error al modificar.");
        }

        [HttpDelete]
        public async Task<IActionResult> Eliminar(int id)
        {
            var claveExistente = await _repoClaves.ObtenerPorIdAsync(id);
            if (claveExistente == null) return NotFound("Clave no encontrada.");

            if (claveExistente.Estado == "Vendida")
                return BadRequest("No se puede eliminar una clave vendida.");

            var exito = await _repoClaves.EliminarAsync(id);
            if (exito) return Ok(new { mensaje = "Clave eliminada." });

            return BadRequest("Error al eliminar.");
        }
    }
}