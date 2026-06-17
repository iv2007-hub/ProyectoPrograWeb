namespace ProyectoPrograWeb.DTOs;

public class ExperimentDTo
{
    //lo que el frontend manda cuando se crea un experimento
    //el userid lo vamos a obtener del token (jnt)
    public string Title { get; set; } = string.Empty;
    public string Result { get; set; } = string.Empty;
    public bool Success { get; set; } = false;
}