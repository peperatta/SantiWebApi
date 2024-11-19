namespace APISensores.Model
{
    // Clase para representar al usuario
    public class Usuario
    {
        public int User_Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Contraseña { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
    }

    // Clase para representar una publicación
    public class PublicacionDTO
    {
        public string Username { get; set; }
        public string Contenido { get; set; }
    }
}
