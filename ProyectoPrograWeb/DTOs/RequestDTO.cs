namespace ProyectoPrograWeb.DTOs;

public class RequestDTO
{
    // identificador unico de la solicitud
    public string Id { get; set; } = string.Empty;
    
    // publicacion a la que pertenece esta solicitud
    public string PostId { get; set; } = string.Empty;
    
    // informacion del receptor que solicito el articulo
    public string ReceiverId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    
    // estado actual: pendiente/aceptada/rechazada/cancelada
    public string Status { get; set; } = "pendiente";
    
    // cuando se envio la solicitud
    public DateTime RequestTimestamp { get; set; }
    
    // cuando el donante respondio
    public DateTime? RespondedAt { get; set; }
}