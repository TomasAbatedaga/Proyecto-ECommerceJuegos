using ECommerceJuegos.Models;
using MySqlConnector;
using ECommerceJuegos.ViewModels;

namespace ECommerceJuegos.Repositories
{
    public class RepositorioResena : IRepositorioResena
    {
        private readonly string _connectionString;

        public RepositorioResena(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<ResenaViewModel>> ObtenerPorProductoAsync(int productoId)
        {
            var resenas = new List<ResenaViewModel>();
            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = @"
                    SELECT r.ResenaId, r.UsuarioId, r.Puntuacion, r.Comentario, r.Fecha, u.Email, u.AvatarUrl 
                    FROM Resenas r
                    INNER JOIN Usuarios u ON r.UsuarioId = u.UsuarioId
                    WHERE r.ProductoId = @productoId
                    ORDER BY r.Fecha DESC;";

                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@productoId", productoId);
                    using (var lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            resenas.Add(new ResenaViewModel
                            {
                                ResenaId = lector.GetInt32("ResenaId"),
                                UsuarioId = lector.GetInt32("UsuarioId"),
                                Puntuacion = lector.GetInt32("Puntuacion"),
                                Comentario = lector.IsDBNull(lector.GetOrdinal("Comentario")) ? null : lector.GetString("Comentario"),
                                Fecha = lector.GetDateTime("Fecha"),
                                UsuarioEmail = lector.GetString("Email"),
                                AvatarUrl = lector.IsDBNull(lector.GetOrdinal("AvatarUrl")) ? null : lector.GetString("AvatarUrl")
                            });
                        }
                    }
                }
            }
            return resenas;
        }

        public async Task<Resena> ObtenerPorUsuarioYProductoAsync(int usuarioId, int productoId)
        {
            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = "SELECT ResenaId, UsuarioId, ProductoId, Puntuacion, Comentario, Fecha FROM Resenas WHERE UsuarioId = @usuarioId AND ProductoId = @productoId;";
                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@usuarioId", usuarioId);
                    comando.Parameters.AddWithValue("@productoId", productoId);
                    using (var lector = await comando.ExecuteReaderAsync())
                    {
                        if (await lector.ReadAsync())
                        {
                            return new Resena
                            {
                                ResenaId = lector.GetInt32("ResenaId"),
                                UsuarioId = lector.GetInt32("UsuarioId"),
                                ProductoId = lector.GetInt32("ProductoId"),
                                Puntuacion = lector.GetInt32("Puntuacion"),
                                Comentario = lector.IsDBNull(lector.GetOrdinal("Comentario")) ? null : lector.GetString("Comentario"),
                                Fecha = lector.GetDateTime("Fecha")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task AgregarAsync(Resena resena)
        {
            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = "INSERT INTO Resenas (UsuarioId, ProductoId, Puntuacion, Comentario, Fecha) VALUES (@usuarioId, @productoId, @puntuacion, @comentario, @fecha);";
                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@usuarioId", resena.UsuarioId);
                    comando.Parameters.AddWithValue("@productoId", resena.ProductoId);
                    comando.Parameters.AddWithValue("@puntuacion", resena.Puntuacion);
                    comando.Parameters.AddWithValue("@comentario", (object)resena.Comentario ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@fecha", DateTime.Now);
                    await comando.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task ModificarAsync(Resena resena)
        {
            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = "UPDATE Resenas SET Puntuacion = @puntuacion, Comentario = @comentario, Fecha = @fecha WHERE ResenaId = @resenaId AND UsuarioId = @usuarioId;";
                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@puntuacion", resena.Puntuacion);
                    comando.Parameters.AddWithValue("@comentario", (object)resena.Comentario ?? DBNull.Value);
                    comando.Parameters.AddWithValue("@fecha", DateTime.Now);
                    comando.Parameters.AddWithValue("@resenaId", resena.ResenaId);
                    comando.Parameters.AddWithValue("@usuarioId", resena.UsuarioId);
                    await comando.ExecuteNonQueryAsync();
                }
            }
        }
    }
}