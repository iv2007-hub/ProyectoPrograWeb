using System.Security.Cryptography;
using System.Text;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;


//en esta clase maneja lo relacionado al registro e inicio de sesion
public class authservice
{
    private readonly firebaseservice _firebaseservice;
    private readonly IConfiguration _configuration;

    public authservice(firebaseservice firebaseservice, IConfiguration configuration)
    {
        _firebaseservice = firebaseservice;
        _configuration = configuration;
        
        
    }

    public async Task<user> Register(registerDTo dto)
    {
        //primero siempre es verificar que no exista un user con ese correo
        var collection = _firebaseservice.GetCollection("user");
        var existing = await collection
            .WhereEqualTo("Email", dto.Email)
            .GetSnapshotAsync();
        if (existing.Count > 0)
            throw new Exception("Ya existe un usuario con este correo");
        
        //darle permiso de que creo un nuevo usuario
        var user = new user
        {
            Id = Guid.NewGuid().ToString(),
            Fullname = dto.FullName,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            Roles = new List<string> { "user" },
            CreatedAt = DateTime.UtcNow
        };
        //
        await collection.Document(user.Id).SetAsync(new Dictionary<string, object>
        {
            { "Id", user.Id },
            { "Fullname", user.Fullname },
            { "Email", user.Email },
            { "Password", user.PasswordHash },
            { "Role", user.Roles },
            { "CreatedAt", user.CreatedAt },

        });
        return user;

    }

    private string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
        
    }
}