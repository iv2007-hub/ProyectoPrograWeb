namespace ProyectoPrograWeb.models;

public class DonationRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    //referencia a la publicacion
    public string PostId { get; set; } = string.Empty;
    
    //informacion del receptor que solicita
    public string ReceiverId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    
    //pendiente/aceptada/rechazada/cancelada
    public string Status { get; set; } = requeststatus.Pendiente;
    
    public DateTime RequestTimestamp { get; set; } = DateTime.UtcNow;
}