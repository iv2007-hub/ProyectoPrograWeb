using Google.Cloud.Firestore;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

public class NotificationService
{
    private readonly FirebaseService _firebaseservice;
    private const string CollectionName = "Notifications";

    public NotificationService(FirebaseService firebaseservice)
    {
        _firebaseservice = firebaseservice;
    }

    private CollectionReference Collection =>
        _firebaseservice.GetCollection(CollectionName);

    public async Task CreateNotification(string userId, string message, string type, string postId)
    {
        var notification = new Notification
        { 
             Id = Guid.NewGuid().ToString(),
            UserId = userId,
            Message = message,
            Type = type,
            PostId = postId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await Collection.Document(notification.Id).SetAsync(notification);
    }

    public async Task<List<Notification>> GetByUser(string userId)
    {
        var snapshot = await Collection
            .WhereEqualTo("UserId", userId)
            .GetSnapshotAsync();

        return snapshot.Documents
            .Select(d => d.ConvertTo<Notification>())
            .OrderByDescending(n => n.CreatedAt)
            .ToList();
    }

    public async Task<bool> MarkAsRead(string id, string userId)
    {
        var doc = await Collection.Document(id).GetSnapshotAsync();
        if (!doc.Exists)
            return false;

        var notification = doc.ConvertTo<Notification>();
        if (notification.UserId != userId)
            return false;

        await Collection.Document(id).UpdateAsync("IsRead", true);
        return true;
    }
}