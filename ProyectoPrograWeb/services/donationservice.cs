using Google.Cloud.Firestore;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

public class Donationservice
{
    private const string CollectionName = "DonationPosts";
    private const string RequestCollectionName = "DonationRequests";
    
    private readonly Firebaseservice _firebaseservice;

    public Donationservice(Firebaseservice firebaseservice)
    {
        _firebaseservice = firebaseservice;
    }
    private CollectionReference Collection => 
        _firebaseservice.GetCollection(CollectionName);
    private CollectionReference RequestCollection => 
        _firebaseservice.GetCollection(RequestCollectionName);
    
    public async Task<Donationpost> CreatePost(CreatedonationpostDTo dto)
    {
        var post = new Donationpost
            
        {
            Id = Guid.NewGuid().ToString(),
            DonorId = dto.DonorId,
            DonorName = dto.DonorName,
            CategoryId = dto.CategoryId,
            ItemName = dto.ItemName,
            Description = dto.Description,
            ItemCondition = dto.ItemCondition,
            Zone = dto.Zone,
            PhotoUrls =  dto.PhotoUrls,
            Status = Donationstatus.Disponible,
            CreatedAt = DateTime.UtcNow
        };
        await Collection.Document(post.Id).SetAsync(post);
        return post;
    }
    
    public async Task<bool> ChangeStatusToReserved(string postId, string requestId)
    {
        var requestRef = RequestCollection.Document(requestId);
        var snapshot = await requestRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            return false;
        }
        var request = snapshot.ConvertTo<DonationRequest>();

        if (request.PostId != postId)
        {
            return false;
        }

        if (request.Status != requeststatus.Pendiente)
        {
            return false;
        }
        var post = await GetEntity(postId);
        if (post == null || post.Status != Donationstatus.Disponible)
        {
            return false;
        }

        request.Status = requeststatus.Aceptada;
        await requestRef.SetAsync(request);

        var pendingRequest = await RequestCollection
            .WhereEqualTo("PostId", postId)
            .WhereEqualTo("Status", requeststatus.Pendiente)
            .GetSnapshotAsync();

        foreach (var doc in pendingRequest.Documents)
        {
            if (doc.Id == requestId)
            {
                continue;
            }

            var otherRequest = doc.ConvertTo<DonationRequest>();
            otherRequest.Status = requeststatus.Rechazada;

            await doc.Reference.SetAsync(otherRequest);
        }

        post.Status = Donationstatus.Reservado;
        post.SelectedReceiverId = request.ReceiverId;
        post.ReservedAt = DateTime.UtcNow;
        
        await Collection.Document(postId).SetAsync(post);
        return true;
    }
    
    private async Task<Donationpost?> GetEntity(string id)
    {
        var doc = await Collection.Document(id).GetSnapshotAsync();

        if (!doc.Exists)
        {
            return null;
        }
        return doc.ConvertTo<Donationpost>();
    }

    public async Task<Donationpost?> GetPostById(string id)
    =>await GetEntity(id);

    public async Task<List<Donationpost>> GetAllActivePost()
    {
        await AutoMarkAsExpired();

        var snapshot = await Collection.GetSnapshotAsync();
            
        return snapshot.Documents
            .Select(d=>d.ConvertTo<Donationpost>())
            .ToList();
    }

    public async Task<List<Donationpost>> GetMyDonor(String donorId)
    {
        var snapshot = await Collection.WhereEqualTo("DonorId", donorId).GetSnapshotAsync();
            
        return snapshot.Documents.Select(d=>d.ConvertTo<Donationpost>()).ToList();
    }

    public async Task<bool> UpdatePost(string id, string donorId, UpdatedonationpostDTo dto)
    {
        var post = await GetEntity(id);
        if (post == null)
        {
            return false;
        }

        if (post.DonorId != donorId)
        {
            return false;
        }

        if (post.Status != Donationstatus.Disponible)
        {
            return false;
        }

        post.CategoryId = dto.CategoryId;
        post.ItemName = dto.ItemName;
        post.Description = dto.Description;
        post.ItemCondition = dto.ItemCondition;
        post.Zone = dto.Zone;
        post.PhotoUrls = dto.PhotoUrls;

        await Collection.Document(id).SetAsync(post);
        return true;
    }

    public async Task<bool> DesactivePost(string id, string donorId)
    {
        var post = await  GetEntity(id);
        if (post == null)
        {
            return false;
        }

        if (post.DonorId != donorId)
        {
            return false;
        }

        if (post.Status != Donationstatus.Disponible)
        {
            return false;
        }

        post.Status = Donationstatus.Vencido;
        await Collection.Document(id).SetAsync(post);
        return true;
    }

    public async Task<bool> ChangeStatusToDelivered(string id)
    {
        var post = await GetEntity(id);
        if (post == null)
        {
            return false;
        }

        if (post.Status != Donationstatus.Reservado)
        {
            return false;
        }

        post.Status = Donationstatus.Entregado;
        post.ClosedAt = DateTime.UtcNow;
        
        await Collection.Document(id).SetAsync(post);
        return true;
    }

    public async Task<List<Donationpost>> GetHistory()
    {
        var snapshot = await Collection.WhereEqualTo("Status", Donationstatus.Entregado).GetSnapshotAsync();
        return snapshot.Documents.Select(d=>d.ConvertTo<Donationpost>()).ToList();
    }

    public async Task AutoMarkAsExpired()
    {
        var snapshot = await Collection
            .WhereEqualTo("Status", Donationstatus.Disponible)
            .GetSnapshotAsync();
        
        var now = DateTime.UtcNow;
        foreach (var doc in snapshot.Documents)
        {
            var post = doc.ConvertTo<Donationpost>();

            if ((now - post.CreatedAt).TotalDays >=30)
            {
                post.Status = Donationstatus.Vencido;
                await doc.Reference.SetAsync(post);
            }
        }
    }
}