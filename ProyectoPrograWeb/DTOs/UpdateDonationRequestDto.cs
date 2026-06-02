namespace ProyectoPrograWeb.DTOs;

public class UpdateDonationRequestDto
{
    // valores permitidos: aceptada, rechazada, cancelada
    public string NuevoEstado { get; set; } = string.Empty;
}