namespace ProyectoPrograWeb.models;

public class experiment
{
    public string Id { get; set; } = string.Empty;
    
    // titulo o nombre al experimento
    public string Title { get; set; } = string.Empty;
    
    // resultado o resumen del experimento
    public string Result { get; set; } = string.Empty;
    
    // guardamos el id del usuario que creo el experimento o prueba
    public string UserId { get; set; } = string.Empty;
    
    // resultado, funciono o no
    public bool Success { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}