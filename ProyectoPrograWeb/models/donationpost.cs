namespace ProyectoPrograWeb.models;

public class donationpost
{
    public string Id { get; set; } = string.Empty;
    
    //informacion del donante
    public string DonorId { get; set; } = string.Empty;
    public string DonorName { get; set; } = string.Empty;
    
    //informacion del articulo
    public string CategoryId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    //nuevo/buen estado/uso regular
    public string ItemCondition { get; set; } = string.Empty;
    
    //zona de entrega
    public string Zone { get; set; } = string.Empty;
    
    //fotos del articulo en Firebase Storage
    public List<string> PhotoUrls { get; set; } = new List<string>();
    
    //disponible/reservado/entregado/vencido
    public string Status { get; set; } = donationstatus.Disponible;
    
    //receptor seleccionado por el donante
    public string SelectedReceiverId { get; set; } = string.Empty;
    
    //timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReservedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}