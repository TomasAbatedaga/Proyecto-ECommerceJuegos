namespace ECommerceJuegos.ViewModels;

public class UsuarioResumenViewModel
{
    public int UsuarioId { get; set; }
    public string Email { get; set; } = "";
    public string Rol { get; set; } = "";
    public string? AvatarUrl { get; set; }
    public bool EstadoActivo { get; set; }
}