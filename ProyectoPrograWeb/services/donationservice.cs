using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

public class donationservice
{
    private readonly firebaseservice _firebaseservice;

    public donationservice(firebaseservice firebaseservice)
    {
        _firebaseservice = firebaseservice;
    }

    public async Task<donationpost> CreatePost(createdonationpostDTo dto)
    {
        var collection = _firebaseservice.GetCollection("DonationPost");

        var post = new donationpost
        {
            Id = Guid.NewGuid().ToString(),
            DonanteId =  dto.DonanteId,
            DonanteNombre = dto.Donantenombre,
            CategoryId =  dto.CategoryId,
            ItemName = dto.Itemname,
            Description = dto.Description,
            ItemCondition =  dto.Itemcondition,
            zona = dto.Zona,
            estatus = "Disponible",
            Created = DateTime.UtcNow
        };

        await collection.Document(post.Id).SetAsync(post);

        return post;
    }
}