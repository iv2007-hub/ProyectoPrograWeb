using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

public class DeliveryService
{
  private readonly firebaseservice _firebaseService;

    public DeliveryService(firebaseservice firebaseService)
    {
        _firebaseService = firebaseService;
    }

    // Busca el post original en su propia colección (no en "Delivery")
    public async Task<DonationPost?> GetPostById(string postId)
    {
        var doc = await _firebaseService.GetCollection("Posts")
            .Document(postId)
            .GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        var data = doc.ToDictionary();

        return new DonationPost
        {
            Id = doc.Id,
            PostId = doc.Id,
            DonorId = data["DonorId"].ToString()!,
            ReceiverId = data["ReceiverId"].ToString()!,
            ItemName = data["ItemName"].ToString()!,
            CategoryId = data["CategoryId"].ToString()!,
        };
    }

    // Para evitar duplicar el log si ya existe uno para ese post
    public async Task<deliveryrecord?> GetByPostId(string postId)
    {
        var snapshot = await _firebaseService.GetCollection("Delivery")
            .WhereEqualTo("PostId", postId)
            .Limit(1)
            .GetSnapshotAsync();

        if (snapshot.Documents.Count == 0)
            return null;

        var data = snapshot.Documents[0].ToDictionary();
        return MapToRecord(data);
    }

    public async Task<deliveryrecord> Create(DeliveryDTO dto, DonationPost post)
    {
        var delivery = new deliveryrecord
        {
            Id = Guid.NewGuid().ToString(),
            PostId = post.PostId,
            DonorId = post.DonorId,
            ReceiverId = post.ReceiverId,
            ItemName = post.ItemName,
            CategoryId = post.CategoryId,
            DeliveryDate = dto.DeliveryDate,
            DeliveryLocation = dto.DeliveryLocation,
            ConfirmedByDonor = dto.ConfirmedByDonor,
            ConfirmedByReceiver = dto.ConfirmedByReceiver,
            CompletedAt = DateTime.UtcNow,
        };

        await _firebaseService.GetCollection("Delivery")
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

    public async Task<List<deliveryrecord>> GetByUser(string userId)
    {
        var asReceiver = await _firebaseService.GetCollection("Delivery")
            .WhereEqualTo("ReceiverId", userId)
            .GetSnapshotAsync();

        var asDonor = await _firebaseService.GetCollection("Delivery")
            .WhereEqualTo("DonorId", userId)
            .GetSnapshotAsync();

        var deliverys = new List<deliveryrecord>();

        foreach (var doc in asReceiver.Documents)
            deliverys.Add(MapToRecord(doc.ToDictionary()));

        foreach (var doc in asDonor.Documents)
            deliverys.Add(MapToRecord(doc.ToDictionary()));

        return deliverys;
    }

    private deliveryrecord MapToRecord(Dictionary<string, object> data)
    {
        return new deliveryrecord
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