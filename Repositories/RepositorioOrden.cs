using ECommerceJuegos.Models;
using ECommerceJuegos.ViewModels;
using MySqlConnector;

namespace ECommerceJuegos.Repositories
{
    public class RepositorioOrden : IRepositorioOrden
    {
        private readonly string _connectionString;

        public RepositorioOrden(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<MisComprasViewModel>> ObtenerComprasPorUsuarioPaginadoAsync(int usuarioId, int pagina, int cantidad)
        {
            var compras = new List<MisComprasViewModel>();
            pagina = Math.Max(1, pagina);
            cantidad = Math.Clamp(cantidad, 1, 50);
            int offset = (pagina - 1) * cantidad;

            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = @"
                    SELECT o.OrdenId, o.Fecha, od.PrecioUnitario, p.Nombre, p.PortadaRuta, cd.Codigo 
                    FROM Ordenes o
                    INNER JOIN OrdenDetalles od ON o.OrdenId = od.OrdenId
                    INNER JOIN Productos p ON od.ProductoId = p.ProductoId
                    INNER JOIN ClavesDigitales cd ON od.ClaveDigitalId = cd.ClaveDigitalId
                    WHERE o.UsuarioId = @usuarioId AND o.Estado = 'Pagada'
                    ORDER BY o.Fecha DESC
                    LIMIT @cantidad OFFSET @offset;";

                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@usuarioId", usuarioId);
                    comando.Parameters.AddWithValue("@cantidad", cantidad);
                    comando.Parameters.AddWithValue("@offset", offset);

                    using (var lector = await comando.ExecuteReaderAsync())
                    {
                        while (await lector.ReadAsync())
                        {
                            compras.Add(new MisComprasViewModel
                            {
                                OrdenId = lector.GetInt32("OrdenId"),
                                Fecha = lector.GetDateTime("Fecha"),
                                PrecioPagado = lector.GetDecimal("PrecioUnitario"),
                                ProductoNombre = lector.GetString("Nombre"),
                                PortadaRuta = lector.IsDBNull(lector.GetOrdinal("PortadaRuta")) ? null : lector.GetString("PortadaRuta"),
                                CodigoClave = lector.GetString("Codigo")
                            });
                        }
                    }
                }
            }
            return compras;
        }

        public async Task<int> ContarComprasPorUsuarioAsync(int usuarioId)
        {
            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = @"
                    SELECT COUNT(1) FROM Ordenes o
                    INNER JOIN OrdenDetalles od ON o.OrdenId = od.OrdenId
                    WHERE o.UsuarioId = @usuarioId AND o.Estado = 'Pagada';";
                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@usuarioId", usuarioId);
                    return Convert.ToInt32(await comando.ExecuteScalarAsync());
                }
            }
        }

        public async Task<bool> UsuarioComproProductoAsync(int usuarioId, int productoId)
        {
            using (var conexion = new MySqlConnection(_connectionString))
            {
                await conexion.OpenAsync();
                string sql = @"
                    SELECT COUNT(1) FROM Ordenes o
                    INNER JOIN OrdenDetalles od ON o.OrdenId = od.OrdenId
                    WHERE o.UsuarioId = @usuarioId AND od.ProductoId = @productoId AND o.Estado = 'Pagada';";
                using (var comando = new MySqlCommand(sql, conexion))
                {
                    comando.Parameters.AddWithValue("@usuarioId", usuarioId);
                    comando.Parameters.AddWithValue("@productoId", productoId);
                    return Convert.ToInt32(await comando.ExecuteScalarAsync()) > 0;
                }
            }
        }
    }
}