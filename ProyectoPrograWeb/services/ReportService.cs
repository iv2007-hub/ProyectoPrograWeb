using ProyectoPrograWeb.DTOs;
using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.services;

public class ReportService
{
    private readonly FirebaseService _firebaseservice;

    public ReportService(FirebaseService firebaseservice)
    {
        _firebaseservice = firebaseservice;
    }

    public async Task<ReportDTO> GetReportAsync(
        string? categoryId = null,
        string? zone = null,
        DateTime? from = null,
        DateTime? to = null)
    {
        var collection = _firebaseservice.GetCollection("DonationPosts");
        var snapshot = await collection.GetSnapshotAsync();

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
            // Aplicar filtro por categoría
            if (!string.IsNullOrEmpty(categoryId))
            {
                var docCategoryId = doc.GetValue<string>("CategoryId") ?? "";
                if (docCategoryId != categoryId) continue;
            }

            // Aplicar filtro por zona
            if (!string.IsNullOrEmpty(zone))
            {
                var docZone = doc.GetValue<string>("Zone") ?? "";
                if (!docZone.ToLower().Contains(zone.ToLower())) continue;
            }

            // Aplicar filtro por fecha
            if (from.HasValue || to.HasValue)
            {
                try
                {
                    var createdAt = doc.GetValue<DateTime>("CreatedAt");
                    if (from.HasValue && createdAt < from.Value) continue;
                    if (to.HasValue && createdAt > to.Value) continue;
                }
                catch { continue; }
            }

            var status = doc.GetValue<string>("Status") ?? "disponible";
            var statusLower = status.ToLower();

            if (!report.ByStatus.ContainsKey(status))
                report.ByStatus[status] = 0;
            report.ByStatus[status]++;

            if (statusLower == "disponible" || statusLower == "reservado")
                report.TotalActive++;
            else if (statusLower == "entregado")
                report.TotalCompleted++;

            var docCategory = doc.GetValue<string>("CategoryId") ?? "";
            var categoryName = categoryNames.ContainsKey(docCategory) ? categoryNames[docCategory] : "sin categoria";
            if (!report.ByCategory.ContainsKey(categoryName))
                report.ByCategory[categoryName] = 0;
            report.ByCategory[categoryName]++;
        }

        // Solicitudes pendientes
        var requestsCollection = _firebaseservice.GetCollection("DonationRequests");
        var requestsSnapshot = await requestsCollection
            .WhereEqualTo("Status", "pendiente")
            .GetSnapshotAsync();
        report.TotalPending = requestsSnapshot.Count;

        // Categorías más solicitadas
        var allRequestsSnapshot = await requestsCollection.GetSnapshotAsync();
        foreach (var doc in allRequestsSnapshot.Documents)
        {
            var postId = doc.GetValue<string>("PostId") ?? "";
            if (string.IsNullOrEmpty(postId)) continue;

            var postDoc = await _firebaseservice.GetCollection("DonationPosts")
                .Document(postId)
                .GetSnapshotAsync();

            if (!postDoc.Exists) continue;

            var docCategory = postDoc.GetValue<string>("CategoryId") ?? "";
            var categoryName = categoryNames.ContainsKey(docCategory) ? categoryNames[docCategory] : "sin categoria";

            if (!report.MostRequested.ContainsKey(categoryName))
                report.MostRequested[categoryName] = 0;
            report.MostRequested[categoryName]++;
        }

        int totalPublicadas = snapshot.Count;
        report.CompletionRate = totalPublicadas > 0
            ? Math.Round((double)report.TotalCompleted / totalPublicadas * 100, 2)
            : 0;

        report.WeeklyTrend = await GetWeeklyTrendAsync();

        return report;
    }

    private async Task<List<WeeklyTrend>> GetWeeklyTrendAsync()
    {
        var deliveryCollection = _firebaseservice.GetCollection("DeliveryRecords");
        var snapshot = await deliveryCollection.GetSnapshotAsync();

        var trend = new Dictionary<string, int>();

        foreach (var doc in snapshot.Documents)
        {
            try
            {
                var completedAt = doc.GetValue<DateTime>("CompletedAt");
                if (completedAt == default) continue;

                var weekStart = completedAt.AddDays(-(int)completedAt.DayOfWeek).ToString("yyyy-MM-dd");

                if (!trend.ContainsKey(weekStart))
                    trend[weekStart] = 0;
                trend[weekStart]++;
            }
            catch
            {
                continue;
            }
        }

        return trend
            .OrderByDescending(x => x.Key)
            .Take(4)
            .OrderBy(x => x.Key)
            .Select(x => new WeeklyTrend { Week = x.Key, Completed = x.Value })
            .ToList();
    }
}