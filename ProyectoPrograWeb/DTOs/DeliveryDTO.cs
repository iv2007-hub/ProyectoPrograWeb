namespace ProyectoPrograWeb.DTOs;

public class DeliveryDTO
{
    public DateTime DeliveryDate { get; set; }
    public string DeliveryLocation { get; set; } = string.Empty;
    public bool ConfirmedByDonor { get; set; }
    public bool ConfirmedByReceiver { get; set; }
    
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}