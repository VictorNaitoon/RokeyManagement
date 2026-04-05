namespace API.DTO.Request.Auditoria
{
    /// <summary>
    /// Filtros para consultar auditoría
    /// </summary>
    public class FiltroAuditoriaRequest
    {
        /// <summary>
        /// Filtrar por nombre de entidad (Venta, Compra, Producto, Presupuesto, Usuario, Caja)
        /// </summary>
        public string? Entidad { get; set; }

        /// <summary>
        /// Filtrar por ID de usuario que realizó la acción
        /// </summary>
        public int? IdUsuario { get; set; }

        /// <summary>
        /// Fecha desde (inclusive)
        /// </summary>
        public DateTime? FechaDesde { get; set; }

        /// <summary>
        /// Fecha hasta (inclusive)
        /// </summary>
        public DateTime? FechaHasta { get; set; }

        /// <summary>
        /// Filtrar por ID del registro modificado
        /// </summary>
        public int? IdRegistro { get; set; }

        /// <summary>
        /// Número de página (default: 1)
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// Tamaño de página (default: 20, max: 100)
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
}