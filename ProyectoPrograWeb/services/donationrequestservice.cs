using Google.Cloud.Firestore;
using ProyectoPrograWeb;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

public class Donationrequestservice
{
    private const string CollectionName = "DonationRequests";
    private readonly Firebaseservice _firebaseservice;

    public Donationrequestservice(Firebaseservice firebaseservice)
    {
        _firebaseservice = firebaseservice;
    }
    
    private CollectionReference Collection =>
        _firebaseservice.GetCollection(CollectionName);
    
    public async Task<bool> CreateRequest(CreateDonationRequestDTo dTo)
    {
        var existing = await Collection
            .WhereEqualTo("PostId", dTo.PostId)
            .WhereEqualTo("ReceiverId", dTo.ReceiverId)
            .WhereEqualTo("Status", requeststatus.Pendiente)
            .GetSnapshotAsync();

        if (existing.Count > 0)
        {
            return false;
        }

        var request = new DonationRequest
        {
            Id = Guid.NewGuid().ToString(),
            PostId = dTo.PostId,
            ReceiverId = dTo.ReceiverId,
            ReceiverName = dTo.ReceiverName,
            Status = requeststatus.Pendiente,
            RequestTimestamp = DateTime.UtcNow
        };

        await Collection.Document(request.Id).SetAsync(request);
        return true;
    }

    public async Task<List<DonationRequest>> GetByPost(string postId)
    {
        var snapshot = await Collection
            .WhereEqualTo("PostId", postId)
            .GetSnapshotAsync();
        return snapshot.Documents.Select(d => d.ConvertTo<DonationRequest>()).ToList();
    }
}