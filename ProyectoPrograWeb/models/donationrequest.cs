namespace ProyectoPrograWeb.models;

public class donationrequest
{
    public string Id { get; set; } = string.Empty;
    
    public string PostId { get; set; } = string.Empty;
    
    public string ReceptorId { get; set; } = string.Empty;
    
    public string Estatus { get; set; } = "Pendiente";
}