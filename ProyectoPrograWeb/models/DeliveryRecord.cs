namespace ProyectoPrograWeb.models;

public class DeliveryRecord
{
    public string Id { get; set; } = string.Empty;
    
    //se hace referencia a la publicacion
    public string PostId { get; set; } = string.Empty;
    
    //la informacion del donante y receptor
    public string DonorId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    
    // la información del articulo entregado
    public string ItemName { get; set; } = string.Empty;
    public string CategoryId { get; set; } = string.Empty;
    
    //coordinacion de la entrega
    public DateTime DeliveryDate { get; set; }
    public string DeliveryLocation { get; set; } = string.Empty;
    
    //la confirmacion doble
    public bool ConfirmedByDonor { get; set; } = false;
    public bool ConfirmedByReceiver { get; set; } = false;
    
    //para mostrar cuando se completo la donacion
    public DateTime CompletedAt { get; set; }
}