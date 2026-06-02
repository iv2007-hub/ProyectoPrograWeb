namespace ProyectoPrograWeb.DTOs;

public class DonationRequestDto
{
    public string Id { get; set; } = string.Empty;
    public string PostId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string Status { get; set; } = "Pendiente";
    public DateTime RequestTimestamp { get; set; }
    public DateTime? RespondedAt { get; set; }
}