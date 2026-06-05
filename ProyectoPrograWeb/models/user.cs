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
    
    //zona de referencia del usuario
    public string Zone { get; set; } = string.Empty;
    
    //roles: donante, receptor o ambos
    public List<string> Roles { get; set; } = new List<string>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    //controla si la cuenta esta activa
    public bool IsActive { get; set; } = true;
}