using ECommerceJuegos.Models;
using ECommerceJuegos.ViewModels;
namespace ECommerceJuegos.Repositories;

public interface IRepositorioCompra
{
    Task<ResultadoCompra> ComprarAsync(int usuarioId, int productoId);
    Task<ConfirmacionCompraViewModel?> ObtenerConfirmacionAsync(int usuarioId, int ordenId);
}
