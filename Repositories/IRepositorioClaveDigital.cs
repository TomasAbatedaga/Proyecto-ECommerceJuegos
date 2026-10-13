using ECommerceJuegos.Models;

namespace ECommerceJuegos.Repositories
{
    public interface IRepositorioClaveDigital
    {
        Task<IEnumerable<ClaveDigital>> ObtenerTodasAsync();
        Task<ClaveDigital?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteCodigoAsync(string codigo);
        Task AgregarAsync(ClaveDigital clave);
        Task<bool> ModificarAsync(ClaveDigital clave);
        Task<bool> EliminarAsync(int id);
    }
}