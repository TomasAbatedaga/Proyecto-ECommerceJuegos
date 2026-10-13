using ECommerceJuegos.Models;
using MySqlConnector;

namespace ECommerceJuegos.Repositories
{
    public class RepositorioClaveDigital : IRepositorioClaveDigital
    {
        private readonly string _connectionString;

        public RepositorioClaveDigital(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<ClaveDigital>> ObtenerTodasAsync()
        {
            var claves = new List<ClaveDigital>();
            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = "SELECT ClaveDigitalId, ProductoId, Codigo, Estado FROM ClavesDigitales ORDER BY ClaveDigitalId DESC;";
                using (var comando = new MySqlCommand(sql, conexion))
                using (var lector = await comando.ExecuteReaderAsync())
                {
                    while (await lector.ReadAsync())
                    {
                        claves.Add(new ClaveDigital
                        {
                            ClaveDigitalId = lector.GetInt32("ClaveDigitalId"),
                            ProductoId = lector.GetInt32("ProductoId"),
                            Codigo = lector.GetString("Codigo"),
                            Estado = lector.GetString("Estado")
                        });
                    }
                }
            }
            return claves;
        }

        public async Task<ClaveDigital?> ObtenerPorIdAsync(int id)
        {
            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = "SELECT ClaveDigitalId, ProductoId, Codigo, Estado FROM ClavesDigitales WHERE ClaveDigitalId = @id;";
                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", id);
                    using (var lector = await comando.ExecuteReaderAsync())
                    {
                        if (await lector.ReadAsync())
                        {
                            return new ClaveDigital
                            {
                                ClaveDigitalId = lector.GetInt32("ClaveDigitalId"),
                                ProductoId = lector.GetInt32("ProductoId"),
                                Codigo = lector.GetString("Codigo"),
                                Estado = lector.GetString("Estado")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<bool> ExisteCodigoAsync(string codigo)
        {
            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = "SELECT COUNT(1) FROM ClavesDigitales WHERE Codigo = @codigo;";
                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@codigo", codigo);
                    return Convert.ToInt32(await comando.ExecuteScalarAsync()) > 0;
                }
            }
        }

        public async Task AgregarAsync(ClaveDigital clave)
        {
            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = "INSERT INTO ClavesDigitales (ProductoId, Codigo, Estado) VALUES (@productoId, @codigo, 'Disponible');";
                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@productoId", clave.ProductoId);
                    comando.Parameters.AddWithValue("@codigo", clave.Codigo);
                    await comando.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<bool> ModificarAsync(ClaveDigital clave)
        {
            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = "UPDATE ClavesDigitales SET Codigo = @codigo WHERE ClaveDigitalId = @id AND Estado = 'Disponible';";
                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@codigo", clave.Codigo);
                    comando.Parameters.AddWithValue("@id", clave.ClaveDigitalId);
                    return await comando.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public async Task<bool> EliminarAsync(int id)
        {
            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = "DELETE FROM ClavesDigitales WHERE ClaveDigitalId = @id AND Estado = 'Disponible';";
                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@id", id);
                    return await comando.ExecuteNonQueryAsync() > 0;
                }
            }
        }
    }
}