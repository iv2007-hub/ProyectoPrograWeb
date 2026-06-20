using Google.Cloud.Firestore;

namespace ProyectoPrograWeb.models;

[FirestoreData]
public class Notification
{
    [FirestoreProperty("Id")]
    public string Id { get; set; } = string.Empty;

    [FirestoreProperty("UserId")]
    public string UserId { get; set; } = string.Empty;

    [FirestoreProperty("Message")]
    public string Message { get; set; } = string.Empty;

    [FirestoreProperty("Type")]
    public string Type { get; set; } = string.Empty;

    [FirestoreProperty("PostId")]
    public string PostId { get; set; } = string.Empty;

    [FirestoreProperty("IsRead")]
    public bool IsRead { get; set; } = false;

    [FirestoreProperty("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}