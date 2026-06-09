using Google.Cloud.Firestore;
using ProyectoPrograWeb;
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
    
    public async Task<bool> CreateRequest(DonationRequest request)
    {
        var existing = await Collection
            .WhereEqualTo("PostId", request.PostId)
            .WhereEqualTo("ReceiverId", request.ReceiverId)
            .WhereEqualTo("Status", requeststatus.Pendiente)
            .GetSnapshotAsync();

        if (existing.Count > 0)
        {
            return false;
        }

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