using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("factura")]
    public class Factura
    {
        /// <summary>
        /// Es el identificador único de la factura, que se genera automáticamente al crear una nueva factura en la base de datos. Este campo es de tipo entero y se utiliza como clave primaria para identificar cada factura de manera única dentro del sistema.
        /// Se autoincrementa para garantizar que cada factura tenga un identificador único y no se repita con otras facturas. Esto es importante para la gestión y la organización.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Es un campo obligatorio que almacena el identificador del negocio al que pertenece la factura. Establece una relación entre la factura y el negocio correspondiente en la base de datos.
        /// </summary>
        [Required]
        public int IdNegocio { get; set; }
        [ForeignKey("IdNegocio")]
        public virtual Negocio Negocio { get; set; } = null!;
        /// <summary>
        /// Es un campo obligatorio que almacena el identificador de la venta asociada a la factura. Establece una relación entre la factura y la venta correspondiente en la base de datos, lo que permite vincular la información de la factura con los detalles de la venta realizada.
        /// </summary>
        [Required]
        public int IdVenta { get; set; }
        [ForeignKey("IdVenta")]
        public virtual Venta Venta { get; set; } = null!;
        /// <summary>
        /// Es opcional y almacena el cuit del cliente al que se le realizó la venta. 
        /// Se utiliza para identificar al cliente de manera única en el sistema, lo que puede ser útil para la gestión de clientes, la generación de informes y el cumplimiento de obligaciones fiscales.
        /// </summary>
        [MaxLength(20)]
        public string? CuitCliente { get; set; }
        /// <summary>
        /// Es un campo obligatorio que almacena la fecha en que se realizó la venta asociada a la factura. 
        /// Este campo es importante para llevar un registro cronológico de las transacciones y para la generación de informes y análisis relacionados con las ventas realizadas por el negocio.
        /// Además, puede ser útil para cumplir con obligaciones fiscales y contables, ya que la fecha de realización de la venta es un dato relevante para determinar el período fiscal correspondiente.
        /// </summary>
        public DateTime FechaRealizada { get; set; }
        /// <summary>
        /// Es un campo obligatorio que almacena el tipo de comprobante de la factura, que puede ser "Factura A", "Factura B", "Factura C", etc.
        /// </summary>
        public Enums.TipoComprobante TipoFactura { get; set; }
        /// <summary>
        /// Es un campo obligatorio que almacena el número de comprobante de la factura, que es un número o código que identifica de manera única a cada factura dentro del sistema. Este número es importante para la gestión y organización de las facturas, así como para cumplir con obligaciones fiscales y contables.
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string NumeroComprobante { get; set; } = string.Empty;
        /// <summary>
        /// Es un campo opcional que almacena el Código de Autorización Electrónico (CAE) de la factura, que es un código generado por la Administración Federal de Ingresos Públicos (AFIP) en Argentina para validar y autorizar electrónicamente las facturas emitidas por los contribuyentes.
        /// </summary>
        [MaxLength(100)]
        public string? CAE { get; set; }
        /// <summary>
        /// Es un campo opcional que almacena la fecha de vencimiento del CAE de la factura, que es la fecha límite hasta la cual el Código de Autorización Electrónico (CAE) es válido para la factura correspondiente. 
        /// Esta fecha es importante para garantizar que las facturas emitidas cumplan con los requisitos fiscales y contables establecidos por la AFIP en Argentina, y para evitar problemas relacionados con la validez de las facturas en caso de que el CAE haya vencido.
        /// </summary>
        public DateTime? VencimientoCAE { get; set; }
        /// <summary>
        /// Es un campo opcional para guardar el código QR de la factura.
        /// </summary>
        public string? QR { get; set; }
        /// <summary>
        /// Es un campo opcional que almacena la condición de venta de la factura, que puede ser "Contado", "Crédito", "Cuenta Corriente", etc. 
        /// Este campo es importante para registrar la forma en que se realizó la venta y para la gestión de las cuentas por cobrar y el seguimiento de los pagos correspondientes a cada factura.
        /// </summary>
        [MaxLength(100)]
        public string? CondicionVenta { get; set; }
    }
}
