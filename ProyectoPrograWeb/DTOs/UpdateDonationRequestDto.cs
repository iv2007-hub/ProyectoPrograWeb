namespace ProyectoPrograWeb.DTOs;

public class UpdateDonationRequestDto
{
    // valores permitidos: aceptada, rechazada, cancelada
    public string Status { get; set; } = string.Empty;
}