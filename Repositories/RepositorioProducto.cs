using ECommerceJuegos.Models;
using ECommerceJuegos.ViewModels;
using MySqlConnector;
namespace ECommerceJuegos.Repositories;

public class RepositorioProducto : RepositorioBase, IRepositorioProducto
{
    private const string Seleccion = @"SELECT p.ProductoId, p.Nombre, p.Descripcion, p.Precio,
  p.CategoriaId, p.EstadoActivo, p.PortadaRuta, c.Nombre AS CategoriaNombre,
  EXISTS (SELECT 1 FROM ClavesDigitales cd WHERE cd.ProductoId = p.ProductoId
    AND cd.Estado = 'Disponible') AS TieneStock
  FROM Productos p INNER JOIN Categorias c ON c.CategoriaId = p.CategoriaId";
    public RepositorioProducto(IConfiguration configuration) : base(configuration) { }
    public int Alta(Producto producto)
    {
        using var cn = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand(@"INSERT INTO Productos
   (Nombre, Descripcion, Precio, CategoriaId, EstadoActivo, PortadaRuta)
   VALUES (@nombre, @descripcion, @precio, @categoria, @activo, @portada);
   SELECT LAST_INSERT_ID();", cn);
        Parametros(cmd, producto); cn.Open();
        return producto.ProductoId = Convert.ToInt32(cmd.ExecuteScalar());
    }
    public int Modificacion(Producto producto, string? portadaAnterior)
    {
        using var cn = new MySqlConnection(ConnectionString);
        // Comparación null-safe: no pisar una portada reemplazada concurrentemente.
        using var cmd = new MySqlCommand(@"UPDATE Productos SET Nombre = @nombre,
   Descripcion = @descripcion, Precio = @precio, CategoriaId = @categoria,
   EstadoActivo = @activo, PortadaRuta = @portada
   WHERE ProductoId = @id AND PortadaRuta <=> @anterior", cn);
        Parametros(cmd, producto);
        cmd.Parameters.AddWithValue("@id", producto.ProductoId);
        cmd.Parameters.AddWithValue("@anterior", (object?)portadaAnterior ?? DBNull.Value);
        cn.Open(); return cmd.ExecuteNonQuery();
    }
    public int BajaLogica(int id)
    {
        using var cn = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand("UPDATE Productos SET EstadoActivo = FALSE WHERE ProductoId = @id", cn);
        cmd.Parameters.AddWithValue("@id", id); cn.Open(); return cmd.ExecuteNonQuery();
    }
    public ProductoListadoViewModel? ObtenerPorId(int id)
    {
        using var cn = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand(Seleccion + " WHERE p.ProductoId = @id", cn);
        cmd.Parameters.AddWithValue("@id", id); cn.Open();
        using var rd = cmd.ExecuteReader(); return rd.Read() ? Leer(rd) : null;
    }
    public int ContarTodos() => Contar(false);
    public int ContarActivos() => Contar(true);
    private int Contar(bool activos)
    {
        using var cn = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Productos" + (activos ? " WHERE EstadoActivo = TRUE" : ""), cn);
        cn.Open(); return Convert.ToInt32(cmd.ExecuteScalar());
    }
    public IList<ProductoListadoViewModel> ObtenerPagina(int pagina, int cantidad) => Pagina(pagina, cantidad, false);
    public IList<ProductoListadoViewModel> ObtenerPaginaActivos(int pagina, int cantidad) => Pagina(pagina, cantidad, true);
    private IList<ProductoListadoViewModel> Pagina(int pagina, int cantidad, bool activos)
    {
        pagina = Math.Max(1, pagina); cantidad = Math.Clamp(cantidad, 1, 50);
        using var cn = new MySqlConnection(ConnectionString);
        using var cmd = new MySqlCommand(Seleccion + (activos ? " WHERE p.EstadoActivo = TRUE" : "") +
         " ORDER BY p.ProductoId LIMIT @cantidad OFFSET @offset", cn);
        cmd.Parameters.AddWithValue("@cantidad", cantidad);
        cmd.Parameters.AddWithValue("@offset", ((long)pagina - 1) * cantidad);
        cn.Open(); using var rd = cmd.ExecuteReader(); var lista = new List<ProductoListadoViewModel>();
        while (rd.Read()) lista.Add(Leer(rd)); return lista;
    }
    private static void Parametros(MySqlCommand cmd, Producto p)
    {
        cmd.Parameters.AddWithValue("@nombre", p.Nombre);
        cmd.Parameters.AddWithValue("@descripcion", p.Descripcion);
        cmd.Parameters.AddWithValue("@precio", p.Precio);
        cmd.Parameters.AddWithValue("@categoria", p.CategoriaId);
        cmd.Parameters.AddWithValue("@activo", p.EstadoActivo);
        cmd.Parameters.AddWithValue("@portada", (object?)p.PortadaRuta ?? DBNull.Value);
    }
    private static ProductoListadoViewModel Leer(MySqlDataReader rd) => new()
    {
        ProductoId = rd.GetInt32(0),
        Nombre = rd.GetString(1),
        Descripcion = rd.GetString(2),
        Precio = rd.GetDecimal(3),
        CategoriaId = rd.GetInt32(4),
        EstadoActivo = rd.GetBoolean(5),
        PortadaRuta = rd.IsDBNull(6) ? null : rd.GetString(6),
        CategoriaNombre = rd.GetString(7),
        TieneStock = rd.GetBoolean(8)
    };
    public async Task<IEnumerable<ProductoBuscadorDTO>> BuscarPorNombreAsync(string query)
{
    var resultados = new List<ProductoBuscadorDTO>();
    using (var conexion = new MySqlConnection(ConnectionString))
    {
        await conexion.OpenAsync();
        string sql = "SELECT ProductoId, Nombre FROM Productos WHERE Nombre LIKE @query AND EstadoActivo = TRUE ORDER BY Nombre, ProductoId LIMIT 10;";
        using (var comando = new MySqlCommand(sql, conexion))
        {
            comando.Parameters.AddWithValue("@query", $"%{query.Trim()}%");
            using (var lector = await comando.ExecuteReaderAsync())
            {
                while (await lector.ReadAsync())
                {
                    resultados.Add(new ProductoBuscadorDTO
                    {
                        ProductoId = lector.GetInt32("ProductoId"),
                        Nombre = lector.GetString("Nombre")
                    });
                }
            }
        }
    }
    return resultados;
}
}
