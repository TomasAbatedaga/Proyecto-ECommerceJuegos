using ECommerceJuegos.Models;

namespace ECommerceJuegos.Repositories;

public interface IRepositorioUsuario
{
    int Alta(Usuario usuario);
    int Baja(int id);
    int Modificacion(Usuario usuario);
    IList<Usuario> ObtenerTodos();
    Usuario? ObtenerPorId(int id);
    Usuario? ObtenerPorEmail(string email);
    bool ExisteEmail(string email, int? excluirUsuarioId = null);
}