namespace ProyectoPrograWeb.DTOs;

public class CreateDonationRequestDTo
{
    // id de la publicacion que el receptor quiere solicitar
    public string PostId { get; set; } = string.Empty;
    
    // id del receptor que esta haciendo la solicitud
    public string ReceiverId { get; set; } = string.Empty;
    
    // nombre del receptor para mostrarlo al donante
    public string ReceiverName { get; set; } = string.Empty;
}