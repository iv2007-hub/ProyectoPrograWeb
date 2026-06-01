namespace ProyectoPrograWeb.DTOs
{
    // Este lo usaremos para enviar datos Limpios al Frontend (ej: en un GET)
    public class categoryresponseDTo
    {
        public string Id { get; set; } = string.Empty;
        
        //nombre de la categoria: ropa, muebles, electronicos, etc
        public string Name { get; set; } = string.Empty;
        
        //el admin puede desactivarla sin borrar publicaciones existentes
        public bool IsActive { get; set; } = true;
    }

    // Este lo usaremos para recibir datos cuando el Frontend cree una categoría nueva (ej: en un POST)
    public class categorycreateDTo
    {
        public string Name { get; set; } = string.Empty;
    }
}
