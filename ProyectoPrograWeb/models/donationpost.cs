namespace ProyectoPrograWeb.models;

public class donationpost
{
    public string Id { get; set; } = string.Empty;
    
    public string DonanteId { get; set; } = string.Empty;
    public string DonanteNombre { get; set; } = string.Empty;
    
    public string CategoryId { get; set; } = string.Empty;
    
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public string ItemCondition { get; set; } = string.Empty;
    
    public string zona { get; set; } = string.Empty;

    public List<string> PhotoUrl { get; set; } = new();
    
    public string estatus { get; set; } = "Disponible";
    
    public string? ReceptorId { get; set; }

    public DateTime Created { get; set; } = DateTime.UtcNow;
    
    public DateTime? reservedAt { get; set; }
    
    public DateTime? ClosedAt { get; set; }
}