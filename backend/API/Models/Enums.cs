namespace API.Models
{
    public class Enums
    {
        public enum EstadoNegocio
        {
            Activo = 1,
            Inactivo = 0
        }
        public enum TipoNegocio
        {
            Cerrajeria = 0,
            Ferreteria = 1,
            MotoRepuestos = 2,
            AutoRepuestos = 3,
        }
        public enum EstadoCategoria 
        {
            Activo = 1,
            Inactivo = 0
        }
        public enum RolUsuario
        {
            Dueño = 1,
            Gerente = 2,
            Empleado = 3
        }
        public enum EstadoPresupuesto
        {
            Pendiente = 1,
            Aceptado = 2,  // Cambiado de "Aprobado" -> "Aceptado"
            Vencido = 3,
            Anulado = 4,
            Rechazado = 5
        }
        public enum MetodoPago
        {
            Efectivo = 1,
            TarjetaCredito = 2,
            TarjetaDebito = 3,
            TransferenciaBancaria = 4,
            MercadoPago = 5
        }
        public enum TipoComprobante
        {
            FacturaA = 1,
            FacturaB = 2,
            FacturaC = 3,
            FacturaM = 4,
            FacturaE = 5,
            FacturaT = 6,
            Recibo = 7,
            NotaCredito = 8
        }
        public enum TipoMovimiento
        {
            Ingreso = 1,
            Egreso = 2,
            Otro = 3,
            
            // Stock management
            VentaSalida = 10,        // Stock deducted when a sale is recorded
            VentaAnulacion = 11,     // Stock restored when a sale is cancelled
            CompraEntrada = 20,      // Stock added when a purchase is recorded
            CompraAnulacion = 21,    // Stock removed when a purchase is cancelled
            AjusteManual = 30        // Manual inventory adjustment
        }

        // --- SAAS / Suscripciones ---
        public enum EstadoSuscripcion
        {
            Activa = 1,
            PendientePago = 2,
            Cancelada = 3,
            Vencida = 4,
            Suspendida = 5
        }

        public enum TipoFacturacion
        {
            Mensual = 1,
            Anual = 2
        }

        public enum MetodoPagoSuscripcion
        {
            TarjetaCredito = 1,
            TarjetaDebito = 2,
            MercadoPago = 3,
            Transferencia = 4
        }

        public enum EstadoPago
        {
            Pendiente = 1,
            Exitoso = 2,
            Fallido = 3,
            Reembolsado = 4
        }

        // --- Actualización Masiva de Precios (CU-010) ---

        public enum TipoActualizacion
        {
            Porcentaje = 1,
            PrecioFijo = 2,
            Incremento = 3
        }

        public enum CampoPrecio
        {
            PrecioVenta = 1,
            PrecioCompra = 2
        }

        // --- Carrito Interno (CU-020) ---
        public enum EstadoCarritoInterno
        {
            Borrador = 1,
            Activo = 2,
            Convertido = 3
        }
    }
}
