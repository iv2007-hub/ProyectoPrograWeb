namespace ProyectoPrograWeb.models;


//representa un experimento o prueba que el usasrio realizo
public class experiment
{
    public string Id { get; set; } = string.Empty;
    
    //titulo del experimento
    public string Title { get; set; } = string.Empty;
    
    //resultado del experimento
    public string Result { get; set; } = string.Empty;
   
    //guardamos el id del usuario que creo el experimento
    public string UserId { get; set; } = string.Empty;
    
    //Resultado, si funciono o no
    public bool Success { get; set; } = false;   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;   
}