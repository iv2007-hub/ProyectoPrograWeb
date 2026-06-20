namespace ProyectoPrograWeb.models;

public class Category
{
    public string Id { get; set; } = string.Empty;
    
    //nombre de la categoria: ropa, muebles, electronicos
    public string Name { get; set; } = string.Empty;
    
    //el admin puede desactivarla sin borrar publicaciones existentes
    public bool IsActive { get; set; } = true;
    
    //conteo de articulos activos en esta categoria
    public int ActiveItemCount { get; set; } = 0;
}