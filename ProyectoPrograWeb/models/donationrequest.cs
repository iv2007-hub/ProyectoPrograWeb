namespace ProyectoPrograWeb.models;
using Google.Cloud.Firestore;
[FirestoreData]
public class DonationRequest
{
    [FirestoreProperty]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    //referencia a la publicacion
    [FirestoreProperty]
    public string PostId { get; set; } = string.Empty;
    
    //informacion del receptor que solicita
    [FirestoreProperty]
    public string ReceiverId { get; set; } = string.Empty;
    [FirestoreProperty]
    public string ReceiverName { get; set; } = string.Empty;
    
    //pendiente/aceptada/rechazada/cancelada
    [FirestoreProperty]
    public string Status { get; set; } = "Pendiente";
    [FirestoreProperty]
    public DateTime RequestTimestamp { get; set; } = DateTime.UtcNow;
}