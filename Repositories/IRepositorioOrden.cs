using ECommerceJuegos.Models;
using ECommerceJuegos.ViewModels;

namespace ECommerceJuegos.Repositories
{
    public interface IRepositorioOrden
    {
        Task<IEnumerable<MisComprasViewModel>> ObtenerComprasPorUsuarioPaginadoAsync(int usuarioId, int pagina, int cantidad);
        Task<int> ContarComprasPorUsuarioAsync(int usuarioId);
        Task<bool> UsuarioComproProductoAsync(int usuarioId, int productoId);
    }
}