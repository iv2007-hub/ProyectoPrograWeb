namespace ProyectoPrograWeb.models;

public class user
{
    //Representa un usuario dentor del sistema
    //esta clase es lo que vamos a guardar en firestore
    
    public string Id { get; set; } = string.Empty;
    
    public string Fullname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    //la contraseña siempre va encriptada/hasheada
    public string PasswordHash { get; set; } = string.Empty;
    
    //por defecto un usuario nuevo sera solo user
    public string Role { get; set; } = "user";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}