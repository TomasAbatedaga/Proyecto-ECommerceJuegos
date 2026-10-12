using ECommerceJuegos.Models;
using MySqlConnector;
namespace ECommerceJuegos.Repositories;

public class RepositorioCategoria : RepositorioBase, IRepositorioCategoria
{
    public RepositorioCategoria(IConfiguration configuration) : base(configuration) { }
    public int Alta(Categoria categoria)
    {
        using var cn = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand("INSERT INTO Categorias (Nombre) VALUES (@nombre); SELECT LAST_INSERT_ID();", cn);
        cmd.Parameters.AddWithValue("@nombre", categoria.Nombre);
        cn.Open();
        return categoria.CategoriaId = Convert.ToInt32(cmd.ExecuteScalar());
    }
    public int Modificacion(Categoria categoria)
    {
        using var cn = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand("UPDATE Categorias SET Nombre = @nombre WHERE CategoriaId = @id", cn);
        cmd.Parameters.AddWithValue("@nombre", categoria.Nombre);
        cmd.Parameters.AddWithValue("@id", categoria.CategoriaId);
        cn.Open(); return cmd.ExecuteNonQuery();
    }
    public int Baja(int id)
    {
        using var cn = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand("DELETE FROM Categorias WHERE CategoriaId = @id", cn);
        cmd.Parameters.AddWithValue("@id", id);
        cn.Open(); return cmd.ExecuteNonQuery();
    }
    public Categoria? ObtenerPorId(int id)
    {
        using var cn = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand("SELECT CategoriaId, Nombre FROM Categorias WHERE CategoriaId = @id", cn);
        cmd.Parameters.AddWithValue("@id", id); cn.Open();
        using var rd = cmd.ExecuteReader();
        return rd.Read() ? new Categoria { CategoriaId = rd.GetInt32(0), Nombre = rd.GetString(1) } : null;
    }
    public IList<Categoria> ObtenerTodos()
    {
        using var cn = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand("SELECT CategoriaId, Nombre FROM Categorias ORDER BY Nombre", cn);
        cn.Open(); using var rd = cmd.ExecuteReader();
        var lista = new List<Categoria>();
        while (rd.Read()) lista.Add(new Categoria { CategoriaId = rd.GetInt32(0), Nombre = rd.GetString(1) });
        return lista;
    }
    public bool ExisteNombre(string nombre, int? excluirCategoriaId = null)
    {
        using var cn = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM Categorias WHERE Nombre = @nombre AND (@excluir IS NULL OR CategoriaId <> @excluir))", cn);
        cmd.Parameters.AddWithValue("@nombre", nombre);
        cmd.Parameters.AddWithValue("@excluir", (object?)excluirCategoriaId ?? DBNull.Value);
        cn.Open(); return Convert.ToBoolean(cmd.ExecuteScalar());
    }
    public bool TieneProductos(int categoriaId)
    {
        using var cn = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand("SELECT EXISTS(SELECT 1 FROM Productos WHERE CategoriaId = @id)", cn);
        cmd.Parameters.AddWithValue("@id", categoriaId);
        cn.Open(); return Convert.ToBoolean(cmd.ExecuteScalar());
    }
}
