namespace ProyectoPrograWeb.models;

public class donationrequest
{
    public string Id { get; set; } = string.Empty;
    
    // referencia a la publicacion
    public string PostId { get; set; } = string.Empty;
    
    // informacion del receptor que solicita
    public string ReceiverId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    
    // pendiente/aceptada/rechazada/cancelada
    public string Status { get; set; } = "pendiente";
    
    public DateTime RequestTimestamp { get; set; } = DateTime.UtcNow;
    
    // cuando el donante respondio la solicitud
    public DateTime? RespondedAt { get; set; }
}