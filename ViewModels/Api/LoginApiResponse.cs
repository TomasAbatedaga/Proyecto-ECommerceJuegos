namespace ECommerceJuegos.ViewModels.Api;

public class LoginApiResponse
{
    public string Token { get; set; } = "";
    public DateTime ExpiraEn { get; set; }
    public int UsuarioId { get; set; }
    public string Email { get; set; } = "";
    public string Rol { get; set; } = "";
}
