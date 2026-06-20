namespace ProyectoPrograWeb.DTOs;

public class SelectReceiverDTo
{
    // id de la solicitud del receptor ganador
    public string RequestId { get; set; } = string.Empty;
    
    // id del receptor que el donante eligio
    public string ReceiverId { get; set; } = string.Empty;
}