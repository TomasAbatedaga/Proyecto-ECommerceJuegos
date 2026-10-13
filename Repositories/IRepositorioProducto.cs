using ECommerceJuegos.Models;
using ECommerceJuegos.ViewModels;
namespace ECommerceJuegos.Repositories;

public interface IRepositorioProducto
{
    int Alta(Producto producto);
    int Modificacion(Producto producto, string? portadaAnterior);
    int BajaLogica(int id);
    ProductoListadoViewModel? ObtenerPorId(int id);
    int ContarTodos();
    IList<ProductoListadoViewModel> ObtenerPagina(int pagina, int cantidad);
    int ContarActivos();
    IList<ProductoListadoViewModel> ObtenerPaginaActivos(int pagina, int cantidad);
    Task<IEnumerable<ProductoBuscadorDTO>> BuscarPorNombreAsync(string query);
}
