using System.ComponentModel.DataAnnotations;

namespace ECommerceJuegos.ViewModels;

public class LoginViewModel
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
    [Required, StringLength(128), DataType(DataType.Password)]
    public string Contrasenia { get; set; } = "";
}