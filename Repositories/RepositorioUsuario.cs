using ECommerceJuegos.Models;
using MySqlConnector;

namespace ECommerceJuegos.Repositories;

public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
{
    public RepositorioUsuario(IConfiguration configuration) : base(configuration) { }

    public int Alta(Usuario usuario)
    {
        using var connection = new MySqlConnection(ConnectionString);
        using var command = new MySqlCommand(@"
            INSERT INTO Usuarios (Email, ContraseniaHash, Rol, AvatarUrl, EstadoActivo)
            VALUES (@email, @hash, @rol, @avatar, @activo);
            SELECT LAST_INSERT_ID();", connection);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@hash", usuario.ContraseniaHash);
        command.Parameters.AddWithValue("@rol", usuario.Rol);
        command.Parameters.AddWithValue("@avatar", (object?)usuario.AvatarUrl ?? DBNull.Value);
        command.Parameters.AddWithValue("@activo", usuario.EstadoActivo);
        connection.Open();
        usuario.UsuarioId = Convert.ToInt32(command.ExecuteScalar());
        return usuario.UsuarioId;
    }

    public int Baja(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        using var command = new MySqlCommand(
            "UPDATE Usuarios SET EstadoActivo = FALSE WHERE UsuarioId = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public int Modificacion(Usuario usuario)
    {
        // La edición administrativa no modifica contraseña ni avatar.
        using var connection = new MySqlConnection(ConnectionString);
        using var command = new MySqlCommand(@"
            UPDATE Usuarios SET Email = @email, Rol = @rol, EstadoActivo = @activo
            WHERE UsuarioId = @id", connection);
        command.Parameters.AddWithValue("@email", usuario.Email);
        command.Parameters.AddWithValue("@rol", usuario.Rol);
        command.Parameters.AddWithValue("@activo", usuario.EstadoActivo);
        command.Parameters.AddWithValue("@id", usuario.UsuarioId);
        connection.Open();
        return command.ExecuteNonQuery();
    }

    public bool ExisteEmail(string email, int? excluirUsuarioId = null)
    {
        using var connection = new MySqlConnection(ConnectionString);
        using var command = new MySqlCommand(@"
            SELECT EXISTS(SELECT 1 FROM Usuarios
                WHERE Email = @email AND (@excluir IS NULL OR UsuarioId <> @excluir))", connection);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@excluir", (object?)excluirUsuarioId ?? DBNull.Value);
        connection.Open();
        return Convert.ToBoolean(command.ExecuteScalar());
    }

    public IList<Usuario> ObtenerTodos()
    {
        using var connection = new MySqlConnection(ConnectionString);
        // El listado no necesita recuperar hashes.
        using var command = new MySqlCommand(@"
            SELECT UsuarioId, Email, Rol, AvatarUrl, EstadoActivo
            FROM Usuarios ORDER BY UsuarioId", connection);
        connection.Open();
        using var reader = command.ExecuteReader();
        var usuarios = new List<Usuario>();
        while (reader.Read()) usuarios.Add(LeerUsuario(reader));
        return usuarios;
    }

    public Usuario? ObtenerPorId(int id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        using var command = new MySqlCommand(@"
            SELECT UsuarioId, Email, Rol, AvatarUrl, EstadoActivo
            FROM Usuarios WHERE UsuarioId = @id", connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();
        using var reader = command.ExecuteReader();
        return reader.Read() ? LeerUsuario(reader) : null;
    }

    public Usuario? ObtenerPorEmail(string email)
    {
        using var connection = new MySqlConnection(ConnectionString);
        using var command = new MySqlCommand(@"
            SELECT UsuarioId, Email, Rol, AvatarUrl, EstadoActivo, ContraseniaHash
            FROM Usuarios WHERE Email = @email", connection);
        command.Parameters.AddWithValue("@email", email);
        connection.Open();
        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;
        var usuario = LeerUsuario(reader);
        usuario.ContraseniaHash = reader.GetString(5);
        return usuario;
    }

    private static Usuario LeerUsuario(MySqlDataReader reader) => new()
    {
        UsuarioId = reader.GetInt32(0),
        Email = reader.GetString(1),
        Rol = reader.GetString(2),
        AvatarUrl = reader.IsDBNull(3) ? null : reader.GetString(3),
        EstadoActivo = reader.GetBoolean(4)
    };
}