namespace ProyectoPrograWeb.models;
using Google.Cloud.Firestore;

[FirestoreData]
public class Donationpost
{
    [FirestoreProperty]
    public string Id { get; set; } = string.Empty;
    [FirestoreProperty]
    //informacion del donante
    public string DonorId { get; set; } = string.Empty;
    [FirestoreProperty]
    public string DonorName { get; set; } = string.Empty;
    
    //informacion del articulo
    [FirestoreProperty]
    public string CategoryId { get; set; } = string.Empty;
    [FirestoreProperty]
    public string ItemName { get; set; } = string.Empty;
    [FirestoreProperty]
    public string Description { get; set; } = string.Empty;
    
    //nuevo/buen estado/uso regular
    [FirestoreProperty]
    public string ItemCondition { get; set; } = string.Empty;
    
    //zona de entrega
    [FirestoreProperty]
    public string Zone { get; set; } = string.Empty;
    
    //fotos del articulo en Firebase Storage
    [FirestoreProperty]
    public List<string> PhotoUrls { get; set; } = new List<string>();
    
    //disponible/reservado/entregado/vencido
    [FirestoreProperty]
    public string Status { get; set; } = Donationstatus.Disponible;
    
    //receptor seleccionado por el donante
    [FirestoreProperty]
    public string SelectedReceiverId { get; set; } = string.Empty;
    
    //timestamps
    [FirestoreProperty]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [FirestoreProperty]
    public DateTime? ReservedAt { get; set; }
    [FirestoreProperty]
    public DateTime? ClosedAt { get; set; }
}