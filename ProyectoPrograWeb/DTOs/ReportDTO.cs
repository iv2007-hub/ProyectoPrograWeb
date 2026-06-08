using ProyectoPrograWeb.models;

namespace ProyectoPrograWeb.DTOs;

// DTO que devuelve el resumen completo de estadísticas para el dashboard del administrador
public class ReportDTO
{
    // totales generales
    public int TotalActive { get; set; } = 0;
    public int TotalCompleted { get; set; } = 0;
    public int TotalPending { get; set; } = 0;

    // porcentaje de donaciones completadas sobre el total publicado
    public double CompletionRate { get; set; } = 0;

    // donaciones agrupadas por categoría
    public Dictionary<string, int> ByCategory { get; set; } = new Dictionary<string, int>();

    // distribución por estado: disponible/reservado/entregado/vencido
    public Dictionary<string, int> ByStatus { get; set; } = new Dictionary<string, int>();

    // tendencia semanal de donaciones completadas
    public List<WeeklyTrend> WeeklyTrend { get; set; } = new List<WeeklyTrend>();
}
