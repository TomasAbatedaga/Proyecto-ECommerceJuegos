using System.Data;
using ECommerceJuegos.Models;
using ECommerceJuegos.ViewModels;
using MySqlConnector;

namespace ECommerceJuegos.Repositories;

public class RepositorioCompra : RepositorioBase, IRepositorioCompra
{
    private readonly ILogger<RepositorioCompra> _logger;
    public RepositorioCompra(IConfiguration configuration, ILogger<RepositorioCompra> logger)
        : base(configuration) => _logger = logger;

    public async Task<ResultadoCompra> ComprarAsync(int usuarioId, int productoId)
    {
        using var conexion = new MySqlConnection(ConnectionString);
        MySqlTransaction? transaccion = null;
        var iniciandoCommit = false;
        try
        {
            await conexion.OpenAsync();
            transaccion = await conexion.BeginTransactionAsync(IsolationLevel.RepeatableRead);

            async Task<ResultadoCompra> Rechazar(EstadoResultadoCompra estado, string mensaje)
            {
                await transaccion.RollbackAsync();
                return new ResultadoCompra(estado, mensaje);
            }

            bool usuarioValido;
            using (var cmd = new MySqlCommand(
                "SELECT UsuarioId, EstadoActivo, Rol FROM Usuarios WHERE UsuarioId = @usuarioId FOR SHARE;",
                conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                using var rd = await cmd.ExecuteReaderAsync();
                usuarioValido = await rd.ReadAsync() && rd.GetBoolean("EstadoActivo") &&
                    rd.GetString("Rol") == "Cliente";
            }
            if (!usuarioValido)
                return await Rechazar(EstadoResultadoCompra.UsuarioInvalido, "El usuario no está habilitado para comprar.");

            decimal precio = 0;
            bool existeProducto;
            bool activo = false;
            using (var cmd = new MySqlCommand(
                "SELECT ProductoId, Nombre, Precio, EstadoActivo FROM Productos WHERE ProductoId = @productoId FOR UPDATE;",
                conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@productoId", productoId);
                using var rd = await cmd.ExecuteReaderAsync();
                existeProducto = await rd.ReadAsync();
                if (existeProducto)
                {
                    precio = rd.GetDecimal("Precio");
                    activo = rd.GetBoolean("EstadoActivo");
                }
            }
            if (!existeProducto)
                return await Rechazar(EstadoResultadoCompra.ProductoNoEncontrado, "Producto no encontrado.");
            if (!activo)
                return await Rechazar(EstadoResultadoCompra.ProductoInactivo, "El producto ya no está disponible.");

            int claveId = 0;
            using (var cmd = new MySqlCommand(
                @"SELECT ClaveDigitalId, ProductoId, Codigo, Estado FROM ClavesDigitales
                  WHERE ProductoId = @productoId AND Estado = 'Disponible'
                  ORDER BY ClaveDigitalId LIMIT 1 FOR UPDATE;", conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@productoId", productoId);
                using var rd = await cmd.ExecuteReaderAsync();
                if (await rd.ReadAsync()) claveId = rd.GetInt32("ClaveDigitalId");
            }
            if (claveId <= 0)
                return await Rechazar(EstadoResultadoCompra.SinStock, "El producto no tiene stock disponible.");

            using (var cmd = new MySqlCommand(
                @"INSERT INTO Ordenes (UsuarioId, Fecha, Estado, MontoTotal)
                  VALUES (@usuarioId, @fecha, 'Pagada', @precio);", conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
                cmd.Parameters.AddWithValue("@fecha", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@precio", precio);
                if (await cmd.ExecuteNonQueryAsync() != 1)
                    throw new InvalidOperationException("La orden no se insertó.");
            }
            int ordenId;
            using (var cmd = new MySqlCommand("SELECT LAST_INSERT_ID();", conexion, transaccion))
                ordenId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            if (ordenId <= 0) throw new InvalidOperationException("Identificador de orden inválido.");

            using (var cmd = new MySqlCommand(
                @"INSERT INTO OrdenDetalles (OrdenId, ProductoId, ClaveDigitalId, PrecioUnitario)
                  VALUES (@ordenId, @productoId, @claveId, @precio);", conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@ordenId", ordenId);
                cmd.Parameters.AddWithValue("@productoId", productoId);
                cmd.Parameters.AddWithValue("@claveId", claveId);
                cmd.Parameters.AddWithValue("@precio", precio);
                if (await cmd.ExecuteNonQueryAsync() != 1)
                    throw new InvalidOperationException("El detalle no se insertó.");
            }
            using (var cmd = new MySqlCommand(
                @"UPDATE ClavesDigitales SET Estado = 'Vendida'
                  WHERE ClaveDigitalId = @claveId AND ProductoId = @productoId AND Estado = 'Disponible';",
                conexion, transaccion))
            {
                cmd.Parameters.AddWithValue("@claveId", claveId);
                cmd.Parameters.AddWithValue("@productoId", productoId);
                if (await cmd.ExecuteNonQueryAsync() != 1)
                    return await Rechazar(EstadoResultadoCompra.ErrorTemporal,
                        "La clave ya no está disponible. Intentá nuevamente.");
            }

            iniciandoCommit = true;
            await transaccion.CommitAsync();
            return new ResultadoCompra(EstadoResultadoCompra.Exito, "Compra realizada correctamente.", ordenId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar la compra. Commit iniciado: {CommitIniciado}", iniciandoCommit);
            if (transaccion != null)
            {
                try { await transaccion.RollbackAsync(); }
                catch (Exception rollbackEx) { _logger.LogError(rollbackEx, "No se pudo confirmar el rollback de compra."); }
            }
            if (iniciandoCommit)
                return new ResultadoCompra(EstadoResultadoCompra.Error,
                    "No se pudo confirmar el resultado de la compra. No la repitas hasta verificar su estado.");
            if (ex is MySqlException mysql && (mysql.Number == 1205 || mysql.Number == 1213))
                return new ResultadoCompra(EstadoResultadoCompra.ErrorTemporal,
                    "No se pudo completar la compra en este momento. Intentá nuevamente.");
            return new ResultadoCompra(EstadoResultadoCompra.Error,
                "No se pudo completar la compra. Intentá nuevamente más tarde.");
        }
        finally
        {
            if (transaccion != null) await transaccion.DisposeAsync();
        }
    }

    public async Task<ConfirmacionCompraViewModel?> ObtenerConfirmacionAsync(int usuarioId, int ordenId)
    {
        using var conexion = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand(
            @"SELECT o.OrdenId, o.Fecha, d.ProductoId, p.Nombre, d.PrecioUnitario, cd.Codigo
              FROM Ordenes o
              INNER JOIN OrdenDetalles d ON d.OrdenId = o.OrdenId
              INNER JOIN Productos p ON p.ProductoId = d.ProductoId
              INNER JOIN ClavesDigitales cd ON cd.ClaveDigitalId = d.ClaveDigitalId
                  AND cd.ProductoId = d.ProductoId
              WHERE o.OrdenId = @ordenId AND o.UsuarioId = @usuarioId AND o.Estado = 'Pagada';",
            conexion);
        cmd.Parameters.AddWithValue("@ordenId", ordenId);
        cmd.Parameters.AddWithValue("@usuarioId", usuarioId);
        await conexion.OpenAsync();
        using var rd = await cmd.ExecuteReaderAsync();
        if (!await rd.ReadAsync()) return null;
        return new ConfirmacionCompraViewModel
        {
            OrdenId = rd.GetInt32("OrdenId"),
            Fecha = DateTime.SpecifyKind(rd.GetDateTime("Fecha"), DateTimeKind.Utc),
            ProductoId = rd.GetInt32("ProductoId"),
            ProductoNombre = rd.GetString("Nombre"),
            PrecioPagado = rd.GetDecimal("PrecioUnitario"),
            CodigoClave = rd.GetString("Codigo")
        };
    }
}
