using System.ComponentModel.DataAnnotations;

namespace API.DTO.Request.Facturas
{
    /// <summary>
    /// Request para emitir una nota de crédito asociada a una venta.
    /// </summary>
    public class NotaCreditoRequest
    {
        /// <summary>
        /// ID de la venta asociada a la nota de crédito.
        /// </summary>
        [Required(ErrorMessage = "El ID de la venta es obligatorio")]
        public int IdVenta { get; set; }

        /// <summary>
        /// Motivo de la nota de crédito (ej: Devolución parcial, Error en factura, etc.)
        /// </summary>
        [Required(ErrorMessage = "El motivo es obligatorio")]
        [MaxLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
        public string Motivo { get; set; } = string.Empty;
    }
}
