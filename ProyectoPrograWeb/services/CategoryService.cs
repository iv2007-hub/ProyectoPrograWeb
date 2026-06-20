using Google.Cloud.Firestore;
using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

public interface ICategoryService
{
    Task<IEnumerable<categoryresponseDTo>> GetAllAsync();
    Task<categoryresponseDTo> CreateAsync(categorycreateDTo dto);
    Task<categoryresponseDTo> UpdateAsync(string id, categoryupdateDTo dto);
    Task<categoryresponseDTo?> ToggleStatusAsync(string id);
    Task<string> GetCategoryNameAsync(string categoryId);
}

public class CategoryService : ICategoryService
{
    private readonly FirebaseService _firebaseService; 

    public CategoryService(FirebaseService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    // Método para obtener todas las categorías desde firebase
    public async Task<IEnumerable<categoryresponseDTo>> GetAllAsync()
    {
        CollectionReference collectionRef = _firebaseService.GetCollection("Categories");
        QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();

        // Obtenemos todas las publicaciones activas de una sola vez
        var postsSnapshot = await _firebaseService.GetCollection("DonationPosts")
            .WhereEqualTo("Status", "Disponible")
            .GetSnapshotAsync();

        // Contamos cuántas publicaciones activas hay por categoría
        var countByCategory = new Dictionary<string, int>();
        foreach (var post in postsSnapshot.Documents)
        {
            var data = post.ToDictionary();
            var categoryId = data.ContainsKey("CategoryId") ? data["CategoryId"].ToString() ?? "" : "";
            if (!string.IsNullOrEmpty(categoryId))
            {
                if (!countByCategory.ContainsKey(categoryId))
                    countByCategory[categoryId] = 0;
                countByCategory[categoryId]++;
            }
        }

        var categoryList = new List<categoryresponseDTo>();
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (document.Exists)
            {
                Dictionary<string, object> data = document.ToDictionary();
                var categoryDto = new categoryresponseDTo
                {
                    Id = document.Id,
                    Name = data.ContainsKey("Name") ? data["Name"].ToString() ?? string.Empty : string.Empty,
                    IsActive = data.ContainsKey("IsActive") ? Convert.ToBoolean(data["IsActive"]) : false,
                    ActiveItemCount = countByCategory.ContainsKey(document.Id) ? countByCategory[document.Id] : 0
                };
                categoryList.Add(categoryDto);
            }
        }

        return categoryList;
    }

    // Crear una nueva categoria
    public async Task<categoryresponseDTo> CreateAsync(categorycreateDTo dto)
    {
        CollectionReference collectionRef = _firebaseService.GetCollection("Categories");

        // Preparamos los datos para firestore 
        Dictionary<string, object> categoryData = new Dictionary<string, object>
        {
            { "Name", dto.Name },
            { "IsActive", true } // Nace activa por defecto
        };

        // Guardamos en firestore y obtenemos la referencia del nuevo documento creado
        DocumentReference docRef = await collectionRef.AddAsync(categoryData);

        // Retornamos el DTo asignando el Id de texto generado en la nube
        return new categoryresponseDTo
        {
            Id = docRef.Id,
            Name = dto.Name,
            IsActive = true
        };
    }
    
    // Metodo completo para editar la categoría en Firestore
    public async Task<categoryresponseDTo?> UpdateAsync(string id, categoryupdateDTo dto)
    {
        // Buscamos el documento específico por su ID único de Firebase
        DocumentReference docRef = _firebaseService.GetCollection("Categories").Document(id);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            return null; // Si no existe, el controlador devolvera un 404 NotFound
        }

        // Actualizamos únicamente el campo name en la nube
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "Name", dto.Name }
        };
        await docRef.UpdateAsync(updates);

        // Mantenemos el estado actual de IsActive que ya tenía en la base de datos
        Dictionary<string, object> currentData = snapshot.ToDictionary();
        bool isActive = currentData.ContainsKey("IsActive") ? Convert.ToBoolean(currentData["IsActive"]) : true;

        return new categoryresponseDTo
        {
            Id = id,
            Name = dto.Name,
            IsActive = isActive
        };
    }

    public async Task<string> GetCategoryNameAsync(string categoryId)
    {
        var collectionRef = _firebaseService.GetCollection("Categories");
        var doc = await collectionRef.Document(categoryId).GetSnapshotAsync();
    
        if (!doc.Exists) return categoryId;
    
        var data = doc.ToDictionary();
        return data.ContainsKey("Name") ? data["Name"].ToString() ?? categoryId : categoryId;
    }
    
    // Metodo completo para activar/desactivar 
    public async Task<categoryresponseDTo?> ToggleStatusAsync(string id)
    {
        DocumentReference docRef = _firebaseService.GetCollection("Categories").Document(id);
        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

        if (!snapshot.Exists)
        {
            return null;
        }

        Dictionary<string, object> currentData = snapshot.ToDictionary();
        bool currentStatus = currentData.ContainsKey("IsActive") ? Convert.ToBoolean(currentData["IsActive"]) : true;
        
        bool newStatus = !currentStatus;

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "IsActive", newStatus }
        };
        await docRef.UpdateAsync(updates);

        return new categoryresponseDTo
        {
            Id = id,
            Name = currentData.ContainsKey("Name") ? currentData["Name"].ToString() ?? string.Empty : string.Empty,
            IsActive = newStatus
        };
    }
}