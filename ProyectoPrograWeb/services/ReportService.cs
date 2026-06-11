using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

// servicio que maneja la generacion de reportes de impacto para el administrador
public class ReportService
{
    private readonly firebaseservice _firebaseservice;

    public ReportService(firebaseservice firebaseservice)
    {
        _firebaseservice = firebaseservice;
    }

    // obtiene el reporte completo de estadisticas
    public async Task<ReportDTO> GetReportAsync()
    {
        var collection = _firebaseservice.GetCollection("DonationPosts");
        var snapshot = await collection.GetSnapshotAsync();

        // obtiene los nombres de categorias para mostrar nombre legible en lugar de ID
        var categoriesCollection = _firebaseservice.GetCollection("Categories");
        var categoriesSnapshot = await categoriesCollection.GetSnapshotAsync();
        var categoryNames = new Dictionary<string, string>();
        foreach (var cat in categoriesSnapshot.Documents)
        {
            var id = cat.Id;
            var name = cat.GetValue<string>("Name") ?? id;
            categoryNames[id] = name;
        }

        var report = new ReportDTO();

        foreach (var doc in snapshot.Documents)
        {
            var status = doc.GetValue<string>("Status") ?? "disponible";

            // conteo por estado
            if (!report.ByStatus.ContainsKey(status))
                report.ByStatus[status] = 0;
            report.ByStatus[status]++;

            // totales generales
            if (status == "disponible" || status == "reservado")
                report.TotalActive++;
            else if (status == "entregado")
                report.TotalCompleted++;

            // conteo por categoria usando nombre legible
            var categoryId = doc.GetValue<string>("CategoryId") ?? "";
            var categoryName = categoryNames.ContainsKey(categoryId) ? categoryNames[categoryId] : "sin categoria";
            if (!report.ByCategory.ContainsKey(categoryName))
                report.ByCategory[categoryName] = 0;
            report.ByCategory[categoryName]++;
        }

        // solicitudes pendientes
        var requestsCollection = _firebaseservice.GetCollection("DonationRequests");
        var requestsSnapshot = await requestsCollection
            .WhereEqualTo("Status", "pendiente")
            .GetSnapshotAsync();
        report.TotalPending = requestsSnapshot.Count;

        // porcentaje de completadas
        int totalPublicadas = snapshot.Count;
        report.CompletionRate = totalPublicadas > 0
            ? Math.Round((double)report.TotalCompleted / totalPublicadas * 100, 2)
            : 0;

        // tendencia semanal de los ultimos 4 periodos
        report.WeeklyTrend = await GetWeeklyTrendAsync();

        return report;
    }

    // calcula la tendencia de donaciones completadas por semana (ultimas 4 semanas)
    private async Task<List<WeeklyTrend>> GetWeeklyTrendAsync()
    {
        var deliveryCollection = _firebaseservice.GetCollection("DeliveryRecords");
        var snapshot = await deliveryCollection.GetSnapshotAsync();

        var trend = new Dictionary<string, int>();

        foreach (var doc in snapshot.Documents)
        {
            try
            {
                // validacion: si CompletedAt es nulo se omite el documento
                var completedAt = doc.GetValue<DateTime>("CompletedAt");
                if (completedAt == default) continue;

                var weekStart = completedAt.AddDays(-(int)completedAt.DayOfWeek).ToString("yyyy-MM-dd");

                if (!trend.ContainsKey(weekStart))
                    trend[weekStart] = 0;
                trend[weekStart]++;
            }
            catch
            {
                // si el campo no existe o es invalido, se omite este registro
                continue;
            }
        }

        // devuelve las ultimas 4 semanas ordenadas
        return trend
            .OrderByDescending(x => x.Key)
            .Take(4)
            .OrderBy(x => x.Key)
            .Select(x => new WeeklyTrend { Week = x.Key, Completed = x.Value })
            .ToList();
    }
}
