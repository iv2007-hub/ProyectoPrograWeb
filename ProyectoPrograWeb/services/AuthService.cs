using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

public class AuthService
{
    private readonly FirebaseService _firebaseService;
    private readonly IConfiguration _configuration;

    public AuthService(FirebaseService firebaseService, IConfiguration configuration)
    {
        _firebaseService = firebaseService;
        _configuration = configuration;
    }

    public async Task<User> Register(RegisterDTo dto)
    {
        // Validar que el rol sea válido
        var rolesValidos = new[] { "Admin", "Donante", "Receptor" };
        if (!rolesValidos.Contains(dto.Role))
            throw new Exception("Rol inválido. Permitidos: Admin, Donante, Receptor");

        // Verificar que no existe un usuario con ese correo
        var collection = _firebaseService.GetCollection("users");
        var existing = await collection
            .WhereEqualTo("Email", dto.Email)
            .GetSnapshotAsync();

        if (existing.Count > 0)
            throw new Exception("Ya existe un usuario con ese correo");

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Fullname = dto.FullName,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            Roles = new List<string> { dto.Role },
            CreatedAt = DateTime.UtcNow
        };

        // Guardar en Firestore
        await collection.Document(user.Id).SetAsync(new Dictionary<string, object>
        {
            { "Id", user.Id },
            { "FullName", user.Fullname },
            { "Email", user.Email },
            { "PasswordHash", user.PasswordHash },
            { "Roles", user.Roles },
            { "CreatedAt", user.CreatedAt }
        });
        
        return user;
    }

    public async Task<string> Login(LoginDTo dto)
    {
        // Buscar usuario por correo
        var collection = _firebaseService.GetCollection("users");
        var snapshot = await collection
            .WhereEqualTo("Email", dto.Email)
            .GetSnapshotAsync();

        if (snapshot.Count == 0)
            throw new Exception("No existe ningún usuario con esa credencial");

        var doc = snapshot.Documents[0];
        var data = doc.ToDictionary();

        var user = new User
        {
            Id = data["Id"].ToString()!,
            Fullname = data["FullName"].ToString()!,
            Email = data["Email"].ToString()!,
            PasswordHash = data["PasswordHash"].ToString()!,
            Roles = ((List<object>)data["Roles"]).Cast<string>().ToList(),
            CreatedAt = ((Google.Cloud.Firestore.Timestamp)data["CreatedAt"]).ToDateTime()
        };

        // Verificar contraseña
        if (!VerifyPassword(dto.Password, user.PasswordHash))
            throw new Exception("Contraseña incorrecta");

        // Generar token
        return GenerateToken(user);
    }

    private string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, string.Join(",", user.Roles))
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private bool VerifyPassword(string dtoPassword, string userPasswordHash)
    {
        return HashPassword(dtoPassword) == userPasswordHash;
    }

    private string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}