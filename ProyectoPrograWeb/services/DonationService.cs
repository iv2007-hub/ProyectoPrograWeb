using Google.Cloud.Firestore;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

public class DonationService
{
    private const string CollectionName = "DonationPosts";
    private const string RequestCollectionName = "DonationRequests";
    
    private readonly FirebaseService _firebaseservice;
    private readonly NotificationService _notificationService;

    public DonationService(FirebaseService firebaseservice, NotificationService notificationService)
    {
        _firebaseservice = firebaseservice;
        _notificationService = notificationService;
    }
    private CollectionReference Collection => 
        _firebaseservice.GetCollection(CollectionName);
    private CollectionReference RequestCollection => 
        _firebaseservice.GetCollection(RequestCollectionName);
    
    public async Task<DonationPost> CreatePost(CreateDonationPostDTo dto)
    {
        var post = new DonationPost
            
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
            Status = DonationStatus.Disponible,
            CreatedAt = DateTime.UtcNow,
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

        if (request.Status != RequestStatus.Pendiente)
        {
            return false;
        }
        var post = await GetEntity(postId);
        if (post == null || post.Status != DonationStatus.Disponible)
        {
            return false;
        }

        request.Status = RequestStatus.Aceptada;
        await requestRef.SetAsync(request);

        var pendingRequest = await RequestCollection
            .WhereEqualTo("PostId", postId)
            .WhereEqualTo("Status", RequestStatus.Pendiente)
            .GetSnapshotAsync();

        foreach (var doc in pendingRequest.Documents)
        {
            if (doc.Id == requestId)
            {
                continue;
            }

            var otherRequest = doc.ConvertTo<DonationRequest>();
            otherRequest.Status = RequestStatus.Rechazada;

            await doc.Reference.SetAsync(otherRequest);
        }

        post.Status = DonationStatus.Reservado;
        post.SelectedReceiverId = request.ReceiverId;
        post.ReservedAt = DateTime.UtcNow;
        
        await Collection.Document(postId).SetAsync(post);
        return true;
    }
    
    private async Task<DonationPost?> GetEntity(string id)
    {
        var doc = await Collection.Document(id).GetSnapshotAsync();

        if (!doc.Exists)
        {
            return null;
        }
        return doc.ConvertTo<DonationPost>();
    }
    
    
    public async Task<bool> AdminDeactivatePost(string id)
    {
        var post = await GetEntity(id);
        if (post == null)
            return false;

        post.Status = DonationStatus.Vencido;
        await Collection.Document(id).SetAsync(post);
        return true;
    }

    public async Task<DonationPost?> GetPostById(string id)
    =>await GetEntity(id);

    public async Task<List<DonationPost>> GetAllActivePost(string? categoryId = null, string? zone = null)
    {
        await AutoMarkAsExpired();
    
        var collection = _firebaseservice.GetCollection(CollectionName);

        var query = await collection.WhereEqualTo("Status", DonationStatus.Disponible).GetSnapshotAsync();

        var posts = query.Documents.Select(d => d.ConvertTo<DonationPost>()).ToList();
  
        if (!string.IsNullOrEmpty(categoryId))
            posts = posts.Where(p => p.CategoryId == categoryId).ToList();

        if (!string.IsNullOrEmpty(zone))
            posts = posts.Where(p => p.Zone.ToLower().Contains(zone.ToLower())).ToList();

        return posts;
    }

    public async Task<List<DonationPost>> GetMyDonor(String donorId)
    {
        var snapshot = await Collection.WhereEqualTo("DonorId", donorId).GetSnapshotAsync();
            
        return snapshot.Documents.Select(d=>d.ConvertTo<DonationPost>()).ToList();
    }

    public async Task<bool> UpdatePost(string id, string donorId, UpdateDonationPostDTo dto)
    {
        var post = await GetEntity(id);
        if (post == null)
            return false;

        if (post.DonorId != donorId)
            return false;

        if (post.Status != DonationStatus.Disponible)
            return false;

        //verificar que no tenga solicitudes activas
        var solicitudesActivas = await RequestCollection
            .WhereEqualTo("PostId", id)
            .WhereIn("Status", new[] { "pendiente", "aceptada" })
            .GetSnapshotAsync();

        if (solicitudesActivas.Count > 0)
            return false;

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

        if (post.Status != DonationStatus.Disponible)
        {
            return false;
        }

        post.Status = DonationStatus.Vencido;
        await Collection.Document(id).SetAsync(post);
        return true;
    }
    
    public async Task<bool> ScheduleDelivery(string id, string donorId, ScheduleDeliveryDTo dto)
    {
        var post = await GetEntity(id);
        if (post == null)
            return false;

        if (post.DonorId != donorId)
            return false;

        if (post.Status != DonationStatus.Reservado)
            return false;

        await Collection.Document(id).UpdateAsync(new Dictionary<string, object>
        {
            { "ScheduledDate", dto.ScheduledDate },
            { "ScheduledLocation", dto.ScheduledLocation }
        });

        await _notificationService.CreateNotification(
            post.SelectedReceiverId,
            $"Se ha coordinado la entrega de: {post.ItemName}. Fecha: {dto.ScheduledDate}, Lugar: {dto.ScheduledLocation}",
            "entrega_coordinada",
            id
        );

        return true;
    }

    public async Task<bool> ChangeStatusToDelivered(string id)
    {
        var post = await GetEntity(id);
        if (post == null)
        {
            return false;
        }

        if (post.Status != DonationStatus.Reservado)
        {
            return false;
        }

        post.Status = DonationStatus.Entregado;
        post.ClosedAt = DateTime.UtcNow;
        
        await Collection.Document(id).SetAsync(post);
        return true;
    }

    public async Task<List<DonationPost>> GetHistory()
    {
        var snapshot = await Collection.WhereEqualTo("Status", DonationStatus.Entregado).GetSnapshotAsync();
        return snapshot.Documents.Select(d=>d.ConvertTo<DonationPost>()).ToList();
    }

    public async Task AutoMarkAsExpired()
    {
        var snapshot = await Collection
            .WhereEqualTo("Status", DonationStatus.Disponible)
            .GetSnapshotAsync();
        
        var now = DateTime.UtcNow;
        foreach (var doc in snapshot.Documents)
        {
            var post = doc.ConvertTo<DonationPost>();

            if ((now - post.CreatedAt).TotalDays >=30)
            {
                post.Status = DonationStatus.Vencido;
                await doc.Reference.SetAsync(post);
            }
        }
    }
}