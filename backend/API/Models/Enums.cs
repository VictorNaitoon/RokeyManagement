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
            Aprobado = 2,
            Vencido = 3
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
            Otro = 3
        }
    }
}
