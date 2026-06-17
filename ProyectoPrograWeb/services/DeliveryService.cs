using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

public class DeliveryService
{
    private readonly FirebaseService _firebaseService;

    public DeliveryService(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    // Busca el post original en su propia colección
    public async Task<DonationPost?> GetPostById(string postId)
    {
        var doc = await _firebaseService.GetCollection("DonationPosts")
            .Document(postId)
            .GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        var data = doc.ToDictionary();

        return new DonationPost
        {
            Id = doc.Id,
            DonorId = data["DonorId"].ToString()!,
            DonorName = data["DonorName"].ToString()!,
            ItemName = data["ItemName"].ToString()!,
            CategoryId = data["CategoryId"].ToString()!,
        };
    }

    // Para evitar duplicar el log si ya existe uno para ese post
    public async Task<DeliveryRecord?> GetByPostId(string postId)
    {
        var snapshot = await _firebaseService.GetCollection("DeliveryRecords")
            .WhereEqualTo("PostId", postId)
            .Limit(1)
            .GetSnapshotAsync();

        if (snapshot.Documents.Count == 0)
            return null;

        var data = snapshot.Documents[0].ToDictionary();
        return MapToRecord(data);
    }

    public async Task<DeliveryRecord> Create(DeliveryDTo dto, DonationPost post)
    {
        var delivery = new DeliveryRecord
        {
            Id = Guid.NewGuid().ToString(),
            PostId = post.Id,
            DonorId = post.DonorId,
            ReceiverId = post.SelectedReceiverId,
            ItemName = post.ItemName,
            CategoryId = post.CategoryId,
            DeliveryDate = dto.DeliveryDate,
            DeliveryLocation = dto.DeliveryLocation,
            ConfirmedByDonor = dto.ConfirmedByDonor,
            ConfirmedByReceiver = dto.ConfirmedByReceiver,
            CompletedAt = DateTime.UtcNow,
        };

        await _firebaseService.GetCollection("DeliveryRecords")
            .Document(delivery.Id)
            .SetAsync(new Dictionary<string, object>
            {
                { "Id", delivery.Id },
                { "PostId", delivery.PostId },
                { "DonorId", delivery.DonorId },
                { "ReceiverId", delivery.ReceiverId },
                { "ItemName", delivery.ItemName },
                { "CategoryId", delivery.CategoryId },
                { "DeliveryDate", delivery.DeliveryDate },
                { "DeliveryLocation", delivery.DeliveryLocation },
                { "ConfirmedByDonor", delivery.ConfirmedByDonor },
                { "ConfirmedByReceiver", delivery.ConfirmedByReceiver },
                { "CompletedAt", delivery.CompletedAt },
            });

        return delivery;
    }

    public async Task<List<DeliveryRecord>> GetByUser(string userId)
    {
        var asReceiver = await _firebaseService.GetCollection("DeliveryRecords")
            .WhereEqualTo("ReceiverId", userId)
            .GetSnapshotAsync();

        var asDonor = await _firebaseService.GetCollection("DeliveryRecords")
            .WhereEqualTo("DonorId", userId)
            .GetSnapshotAsync();

        var deliverys = new List<DeliveryRecord>();

        foreach (var doc in asReceiver.Documents)
            deliverys.Add(MapToRecord(doc.ToDictionary()));

        foreach (var doc in asDonor.Documents)
            deliverys.Add(MapToRecord(doc.ToDictionary()));

        return deliverys;
    }

    private DeliveryRecord MapToRecord(Dictionary<string, object> data)
    {
        return new DeliveryRecord
        {
            Id = data["Id"].ToString()!,
            PostId = data["PostId"].ToString()!,
            DonorId = data["DonorId"].ToString()!,
            ReceiverId = data["ReceiverId"].ToString()!,
            ItemName = data["ItemName"].ToString()!,
            CategoryId = data["CategoryId"].ToString()!,
            DeliveryDate = ((Google.Cloud.Firestore.Timestamp)data["DeliveryDate"]).ToDateTime(),
            DeliveryLocation = data["DeliveryLocation"].ToString()!,
            ConfirmedByDonor = (bool)data["ConfirmedByDonor"],
            ConfirmedByReceiver = (bool)data["ConfirmedByReceiver"],
            CompletedAt = ((Google.Cloud.Firestore.Timestamp)data["CompletedAt"]).ToDateTime()
        };
    }
}