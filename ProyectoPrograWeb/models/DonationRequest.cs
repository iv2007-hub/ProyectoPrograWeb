using Google.Cloud.Firestore;

namespace ProyectoPrograWeb.models;

[FirestoreData]
public class DonationRequest
{
    [FirestoreProperty("Id")]
    public string Id { get; set; } = string.Empty;
    
    [FirestoreProperty("PostId")]
    public string PostId { get; set; } = string.Empty;
    
    [FirestoreProperty("ReceiverId")]
    public string ReceiverId { get; set; } = string.Empty;
    
    [FirestoreProperty("ReceiverName")]
    public string ReceiverName { get; set; } = string.Empty;
    //pendienete/aceptado/rechazado o cancelado
    [FirestoreProperty("Status")]
    public string Status { get; set; } = "pendiente";
    
    [FirestoreProperty("RequestTimestamp")]
    public DateTime RequestTimestamp { get; set; } = DateTime.UtcNow;
    
    [FirestoreProperty("RespondedAt")]
    public DateTime? RespondedAt { get; set; }
}