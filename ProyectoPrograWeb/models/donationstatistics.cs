namespace ProyectoPrograWeb.models;

public class donationstatistics
{
    //se muestra totales generales
    public int TotalActive { get; set; } = 0;
    public int TotalCompleted { get; set; } = 0;
    public int TotalPending { get; set; } = 0;
    
    //los porcentaje de donaciones hechas
    public double CompletionRate { get; set; } = 0;
    
    //las donaciones son agrupadas por categoria
    public Dictionary<string, int> ByCategory { get; set; } = new Dictionary<string, int>();
    
    //la distribucion es por estado, ya sea disponible/reservado/entregado/vencido
    public Dictionary<string, int> ByStatus { get; set; } = new Dictionary<string, int>();
    
    //cuantas donaciones semanales son completadas
    public List<WeeklyTrend> WeeklyTrend { get; set; } = new List<WeeklyTrend>();
}

public class WeeklyTrend
{
    public string Week { get; set; } = string.Empty;
    public int Completed { get; set; } = 0;
}