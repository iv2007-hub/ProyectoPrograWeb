using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

public class experimentservice
{
    
    private readonly firebaseservice _firebaseService;

        public experimentservice(firebaseservice firebaseService)
        {
            _firebaseService = firebaseService;
        }

        public async Task<experiment> Create(experimentDTo dto, string userId)
        {
            var experiment = new experiment
            {
                Id = Guid.NewGuid().ToString(),
                Title = dto.Title,
                Result = dto.Result,
                Success = dto.Success,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            // Guardamos con Dictionary explícito - evitamos el problema
            // de ConvertTo<T>() que ya conocemos del proyecto anterior
            await _firebaseService.GetCollection("experiments")
                .Document(experiment.Id)
                .SetAsync(new Dictionary<string, object>
                {
                    { "Id", experiment.Id },
                    { "Title", experiment.Title },
                    { "Result", experiment.Result },
                    { "Success", experiment.Success },
                    { "UserId", experiment.UserId },
                    { "CreatedAt", experiment.CreatedAt }
                });

            return experiment;
        }

        public async Task<List<experiment>> GetByUser(string userId)
        {
            // Solo traemos los experimentos del usuario que está logueado
            // No queremos que un usuario vea los experimentos de otro
            var snapshot = await _firebaseService.GetCollection("experiments")
                .WhereEqualTo("UserId", userId)
                .GetSnapshotAsync();

            var experiments = new List<experiment>();

            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();

                experiments.Add(new experiment
                {
                    Id = data["Id"].ToString()!,
                    Title = data["Title"].ToString()!,
                    Result = data["Result"].ToString()!,
                    // Firestore devuelve Int64 para números, bool lo maneja bien
                    Success = (bool)data["Success"],
                    UserId = data["UserId"].ToString()!,
                    CreatedAt = ((Google.Cloud.Firestore.Timestamp)data["CreatedAt"]).ToDateTime()
                });
            }

            return experiments;
        }
}