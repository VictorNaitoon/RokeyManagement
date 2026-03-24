namespace API.DTO.Request.Categorias
{
    public class CrearCategoriaRequest
    {
        /// <summary>
        /// Nombre de la categoría (obligatorio, máximo 100 caracteres)
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción opcional de la categoría (máximo 500 caracteres)
        /// </summary>
        public string? Descripcion { get; set; }
    }

    public class ActualizarCategoriaRequest
    {
        /// <summary>
        /// Nombre de la categoría (obligatorio, máximo 100 caracteres)
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Descripción opcional de la categoría (máximo 500 caracteres)
        /// </summary>
        public string? Descripcion { get; set; }

        /// <summary>
        /// Indica si la categoría está activa o inactiva (para soft delete y reactivación)
        /// </summary>
        public bool Activo { get; set; } = true;
    }
}