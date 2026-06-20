namespace ProyectoPrograWeb.DTOs;

public class RegisterDTo
{
    //lo que el frontend desde una interfaz va a enviar 
    //cuando se quiera registrar

    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}