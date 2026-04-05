using System.ComponentModel.DataAnnotations;
using API.Models;

namespace API.DTO.Request.Facturas
{
    /// <summary>
    /// Request para crear una factura proforma asociada a una venta.
    /// </summary>
    public class CrearFacturaRequest
    {
        /// <summary>
        /// ID de la venta asociada a la factura.
        /// </summary>
        [Required(ErrorMessage = "El ID de la venta es obligatorio")]
        public int IdVenta { get; set; }

        /// <summary>
        /// CUIT del cliente en formato XX-XXXXXXXX-X
        /// </summary>
        [Required(ErrorMessage = "El CUIT del cliente es obligatorio")]
        [MaxLength(20, ErrorMessage = "El CUIT no puede exceder 20 caracteres")]
        public string CUIT_cliente { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de comprobante a emitir (FacturaA, FacturaB, etc.)
        /// </summary>
        [Required(ErrorMessage = "El tipo de comprobante es obligatorio")]
        public Enums.TipoComprobante TipoComprobante { get; set; }

        /// <summary>
        /// Condición de venta (Contado, Crédito, Cuenta Corriente)
        /// </summary>
        [Required(ErrorMessage = "La condición de venta es obligatoria")]
        [MaxLength(100, ErrorMessage = "La condición de venta no puede exceder 100 caracteres")]
        public string CondicionVenta { get; set; } = string.Empty;
    }
}
