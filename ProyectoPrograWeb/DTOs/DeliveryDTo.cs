namespace ProyectoPrograWeb.DTOs;

public class DeliveryDTo
{
    public DateTime DeliveryDate { get; set; }
    public string DeliveryLocation { get; set; } = string.Empty;
    public bool ConfirmedByDonor { get; set; }
    public bool ConfirmedByReceiver { get; set; }
}