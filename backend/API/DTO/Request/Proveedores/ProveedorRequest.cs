namespace API.DTO.Request.Proveedores
{
    public class CrearProveedorRequest
    {
        /// <summary>
        /// Nombre o razón social del proveedor (obligatorio, máximo 200 caracteres)
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono del proveedor (opcional, máximo 50 caracteres)
        /// </summary>
        public string? Telefono { get; set; }

        /// <summary>
        /// Correo electrónico del proveedor (opcional)
        /// </summary>
        public string? Email { get; set; }
    }

    public class ActualizarProveedorRequest
    {
        /// <summary>
        /// Nombre o razón social del proveedor (obligatorio, máximo 200 caracteres)
        /// </summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>
        /// Teléfono del proveedor (opcional, máximo 50 caracteres)
        /// </summary>
        public string? Telefono { get; set; }

        /// <summary>
        /// Correo electrónico del proveedor (opcional)
        /// </summary>
        public string? Email { get; set; }
    }
}