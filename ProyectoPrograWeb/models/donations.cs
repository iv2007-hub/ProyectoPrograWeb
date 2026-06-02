namespace ProyectoPrograWeb.models;

public class donations
{
    public string id { get; set; } = string.Empty;
    
    public string userid { get; set; } = string.Empty;
    
    public decimal monto { get; set; }
    
    public string mensaje { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}