using API.Models;

namespace API.DTO.Request.Productos
{
    /// <summary>
    /// Request para actualización masiva de precios de productos por categoría (CU-010)
    /// </summary>
    public class ActualizacionMasivaPreciosCategoriaRequest
    {
        /// <summary>
        /// ID de la categoría a actualizar. Requerido.
        /// </summary>
        public int IdCategoria { get; set; }
        
        /// <summary>
        /// Tipo de actualización a realizar.
        /// Valores: 1 = Porcentaje, 2 = PrecioFijo, 3 = Incremento.
        /// </summary>
        public Enums.TipoActualizacion TipoActualizacion { get; set; }
        
        /// <summary>
        /// Porcentaje de aumento o reducción.
        /// Ejemplo: 15 = +15%, -10 = -10%.
        /// Usar cuando TipoActualizacion = 1 (Porcentaje).
        /// </summary>
        public decimal? Porcentaje { get; set; }
        
        /// <summary>
        /// Precio fijo en pesos a establecer.
        /// Usar cuando TipoActualizacion = 2 (PrecioFijo).
        /// </summary>
        public decimal? PrecioFijo { get; set; }
        
        /// <summary>
        /// Monto en pesos a incrementar (sumar al precio actual).
        /// Usar cuando TipoActualizacion = 3 (Incremento).
        /// </summary>
        public decimal? Incremento { get; set; }
        
        /// <summary>
        /// Campo de precio a actualizar.
        /// Valores: 1 = PrecioVenta, 2 = PrecioCompra.
        /// </summary>
        public Enums.CampoPrecio CampoPrecio { get; set; }
    }
}
