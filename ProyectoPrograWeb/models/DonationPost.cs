using Google.Cloud.Firestore;

namespace ProyectoPrograWeb.models;

[FirestoreData]
public class DonationPost
{
    [FirestoreProperty("Id")]
    public string Id { get; set; } = string.Empty;
    
    [FirestoreProperty("DonorId")]
    public string DonorId { get; set; } = string.Empty;
    
    [FirestoreProperty("DonorName")]
    public string DonorName { get; set; } = string.Empty;
    
    [FirestoreProperty("CategoryId")]
    public string CategoryId { get; set; } = string.Empty;
    
    [FirestoreProperty("ItemName")]
    public string ItemName { get; set; } = string.Empty;
    
    [FirestoreProperty("Description")]
    public string Description { get; set; } = string.Empty;
    
    [FirestoreProperty("ItemCondition")]
    public string ItemCondition { get; set; } = string.Empty;
    
    [FirestoreProperty("Zone")]
    public string Zone { get; set; } = string.Empty;
    
    [FirestoreProperty("PhotoUrls")]
    public List<string> PhotoUrls { get; set; } = new List<string>();
    
    [FirestoreProperty("Status")]
    public string Status { get; set; } = "Disponible";
    
    [FirestoreProperty("SelectedReceiverId")]
    public string SelectedReceiverId { get; set; } = string.Empty;
    
    [FirestoreProperty("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [FirestoreProperty("ReservedAt")]
    public DateTime? ReservedAt { get; set; }
    
    [FirestoreProperty("ClosedAt")]
    public DateTime? ClosedAt { get; set; }
    
    [FirestoreProperty("ScheduledDate")]
    public DateTime? ScheduledDate { get; set; }

    [FirestoreProperty("ScheduledLocation")]
    public string ScheduledLocation { get; set; } = string.Empty;
    
}

