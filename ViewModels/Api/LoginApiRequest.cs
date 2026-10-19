using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.ViewModels.Api;

public class LoginApiRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";

    [Required]
    public string Contrasenia { get; set; } = "";
}
