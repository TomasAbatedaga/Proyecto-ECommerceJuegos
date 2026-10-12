using ECommerceJuegos.Models;
namespace ECommerceJuegos.Repositories;

public interface IRepositorioCategoria
{
    int Alta(Categoria categoria);
    int Modificacion(Categoria categoria);
    int Baja(int id);
    Categoria? ObtenerPorId(int id);
    IList<Categoria> ObtenerTodos();
    bool ExisteNombre(string nombre, int? excluirCategoriaId = null);
    bool TieneProductos(int categoriaId);
}
