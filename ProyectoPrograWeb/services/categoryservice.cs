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
}

public class categoryservice : ICategoryService
{
    private readonly firebaseservice _firebaseService; 

    public categoryservice(firebaseservice firebaseService)
    {
        _firebaseService = firebaseService;
    }

    // Método para obtener todas las categorías desde firebase
    public async Task<IEnumerable<categoryresponseDTo>> GetAllAsync()
    {
        // Apuntamos a la colección llamada "Categories" usando el método GetCollection
        CollectionReference collectionRef = _firebaseService.GetCollection("Categories");
        
        // Consultamos todos los documentos de esa colección de manera asíncrona
        QuerySnapshot snapshot = await collectionRef.GetSnapshotAsync();

        var categoryList = new List<categoryresponseDTo>();

        // Recorremos cada documento que nos devolvió firestore
        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            if (document.Exists)
            {
                // Convertimos el documento de firestore en un diccionario de datos
                Dictionary<string, object> data = document.ToDictionary();

                // Extraemos los datos validando que existan y mapeamos al DTo
                var categoryDto = new categoryresponseDTo
                {
                    Id = document.Id, //Usamos el ID string nativo de firestore
                    Name = data.ContainsKey("Name") ? data["Name"].ToString() ?? string.Empty : string.Empty,
                    IsActive = data.ContainsKey("IsActive") ? Convert.ToBoolean(data["IsActive"]) : false
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