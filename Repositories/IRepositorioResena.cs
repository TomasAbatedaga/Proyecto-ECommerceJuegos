using ECommerceJuegos.Models;
using ECommerceJuegos.ViewModels;

namespace ECommerceJuegos.Repositories
{
    public interface IRepositorioResena
    {
        Task<IEnumerable<ResenaViewModel>> ObtenerPorProductoAsync(int productoId);
        Task<Resena?> ObtenerPorUsuarioYProductoAsync(int usuarioId, int productoId);
        Task AgregarAsync(Resena resena);
        Task ModificarAsync(Resena resena);
    }
}