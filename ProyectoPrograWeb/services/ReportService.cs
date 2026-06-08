using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

// servicio que maneja la generación de reportes de impacto para el administrador
public class ReportService
{
    private readonly firebaseservice _firebaseservice;

    public ReportService(firebaseservice firebaseservice)
    {
        _firebaseservice = firebaseservice;
    }

    // obtiene el reporte completo de estadísticas
    public async Task<ReportDTO> GetReportAsync()
    {
        var collection = _firebaseservice.GetCollection("DonationPosts");
        var snapshot = await collection.GetSnapshotAsync();

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

            // conteo por categoría
            var categoryId = doc.GetValue<string>("CategoryId") ?? "sin categoría";
            if (!report.ByCategory.ContainsKey(categoryId))
                report.ByCategory[categoryId] = 0;
            report.ByCategory[categoryId]++;
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

        // tendencia semanal de los últimos 4 períodos
        report.WeeklyTrend = await GetWeeklyTrendAsync();

        return report;
    }

    // calcula la tendencia de donaciones completadas por semana (últimas 4 semanas)
    private async Task<List<WeeklyTrend>> GetWeeklyTrendAsync()
    {
        var deliveryCollection = _firebaseservice.GetCollection("DeliveryRecords");
        var snapshot = await deliveryCollection.GetSnapshotAsync();

        var trend = new Dictionary<string, int>();

        foreach (var doc in snapshot.Documents)
        {
            // agrupa por semana usando el campo completedAt
            var completedAt = doc.GetValue<DateTime>("CompletedAt");
            var weekStart = completedAt.AddDays(-(int)completedAt.DayOfWeek).ToString("yyyy-MM-dd");

            if (!trend.ContainsKey(weekStart))
                trend[weekStart] = 0;
            trend[weekStart]++;
        }

        // devuelve las últimas 4 semanas ordenadas
        return trend
            .OrderByDescending(x => x.Key)
            .Take(4)
            .OrderBy(x => x.Key)
            .Select(x => new WeeklyTrend { Week = x.Key, Completed = x.Value })
            .ToList();
    }
}
