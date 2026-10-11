using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ECommerceJuegos.ViewModels;

public class PerfilViewModel
{
    [BindNever]
    public string Email { get; set; } = "";
    [BindNever]
    public string Rol { get; set; } = "";
    [BindNever]
    public bool EstadoActivo { get; set; }
    [BindNever]
    public string? AvatarUrl { get; set; }

    public IFormFile? Avatar { get; set; }
}