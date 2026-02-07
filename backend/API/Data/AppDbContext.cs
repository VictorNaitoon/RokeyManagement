using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Negocio> Negocios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Compra> Compras { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }
        public DbSet<MovimientoStock> MovimientosStock { get; set; }
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<Presupuesto> Presupuestos { get; set; }
        public DbSet<DetallePresupuesto> DetallesPresupuesto { get; set; }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<DateTime>()
                .HaveColumnType("timestamp with time zone"); // Configura el tipo de dato para DateTime en PostgreSQL
        }

        /// <summary>
        /// La configuración adicional es para el método de borrado, algunos en cascada o por defecto, 
        /// y otros con restricción para evitar la eliminación de registros relacionados. 
        /// Esto es importante para mantener la integridad de los datos y evitar la eliminación accidental 
        /// de registros relacionados que podrían afectar la consistencia de la base de datos.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Configuración adicional si es necesario
            /// <summary>
            /// Regla General: Evitar borrados en cascada para mantener el historial contable.
            /// </summary>
            // --- VENTA ---
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.IdCliente)
                .OnDelete(DeleteBehavior.Restrict); // No borrar la venta si el cliente es borrado.
            
            modelBuilder.Entity<Venta>()
                .HasOne(v => v.Usuario)
                .WithMany(u => u.Ventas)
                .HasForeignKey(v => v.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict); // No borrar la venta si el usuario es borrado, para auditoría y mantener quién vendió.

            // --- COMPRA ---
            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Compras)
                .HasForeignKey(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict); // No borrar la compra si el usuario es borrado, para auditoría y mantener quién compró.

            modelBuilder.Entity<Compra>()
                .HasOne(c => c.Proveedor)
                .WithMany(p => p.Compras)
                .HasForeignKey(c => c.IdProveedor)
                .OnDelete(DeleteBehavior.Restrict); // No borrar la compra si el proveedor es borrado, para mantener el historial de compras de ese proveedor.

            // --- PRESUPUESTO ---
            modelBuilder.Entity<Presupuesto>()
                .HasOne(p => p.Cliente)
                .WithMany(c => c.Presupuestos)
                .HasForeignKey(p => p.IdCliente)
                .OnDelete(DeleteBehavior.Restrict); // No borrar el presupuesto si el cliente es borrado.
            
            modelBuilder.Entity<Presupuesto>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Presupuestos)
                .HasForeignKey(p => p.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict); // No borrar el presupuesto si el usuario es borrado, para mantener quién hizo el presupuesto.

            // --- PRODUCTOS --- (Núcleo del historial)
            /// <summary>
            /// Evitamos que al borrar un producto desaparezcan sus registros históricos.
            /// </summary>
            modelBuilder.Entity<DetalleVenta>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict); // No borrar el detalle de venta si el producto es borrado, para mantener el historial de ventas de ese producto.

            modelBuilder.Entity<DetalleCompra>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesCompra)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict); // No borrar el detalle de compra si el producto es borrado, para mantener el historial de compras de ese producto.

            modelBuilder.Entity<MovimientoStock>()
                .HasOne(m => m.Producto)
                .WithMany(p => p.MovimientosStock)
                .HasForeignKey(m => m.IdProducto)
                .OnDelete(DeleteBehavior.Restrict); // No borrar el movimiento de stock si el producto es borrado, para mantener el historial de movimientos de ese producto.

            modelBuilder.Entity<DetallePresupuesto>()
                .HasOne(d => d.Presupuesto)
                .WithMany(p => p.DetallesPresupuesto)
                .HasForeignKey(d => d.IdPresupuesto)
                .OnDelete(DeleteBehavior.Restrict); // No borrar el detalle de presupuesto si el presupuesto es borrado, para mantener el historial de presupuestos de ese producto.

            modelBuilder.Entity<DetallePresupuesto>()
                .HasOne(d => d.Producto)
                .WithMany(p => p.DetallesPresupuesto)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict); // No borrar el detalle de presupuesto si el producto es borrado, para mantener el historial de presupuestos de ese producto.

            // --- MOVIMIENTOS DE STOCK ---
            /// <summary>
            /// No se debe borrar un movimiento de stock, esto es una auditoría estricta.
            /// </summary>
            modelBuilder.Entity<MovimientoStock>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.MovimientosStock)
                .HasForeignKey(m => m.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict); // No borrar el movimiento de stock si el usuario es borrado, para mantener el historial de quién realizó cada movimiento de stock.

            /// <summary>
            /// Las relaciones con Venta y Compra son opcionales (pueden ser null), pero si existen, aplicamos la restricción.
            /// </summary>
            modelBuilder.Entity<MovimientoStock>()
                .HasOne(m => m.Venta)
                .WithMany(v => v.MovimientosStock)
                .HasForeignKey(m => m.IdVenta)
                .OnDelete(DeleteBehavior.Restrict); // No borrar el movimiento de stock si la venta es borrada, para mantener el historial de movimientos relacionados con esa venta.

            modelBuilder.Entity<MovimientoStock>()
                .HasOne(m => m.Compra)
                .WithMany(c => c.MovimientosStock)
                .HasForeignKey(m => m.IdCompra)
                .OnDelete(DeleteBehavior.Restrict); // No borrar el movimiento de stock si la compra es borrada, para mantener el historial de movimientos relacionados con esa compra.

            // --- FACTURACIÓN ---
            modelBuilder.Entity<Factura>()
                .HasOne(f => f.Venta)
                .WithMany(v => v.Facturas)
                .HasForeignKey(f => f.IdVenta)
                .OnDelete(DeleteBehavior.Restrict); // No borrar la factura si la venta es borrada, para mantener el historial de facturación de esa venta.

            // --- CATEGORIA ---
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.IdCategoria)
                .OnDelete(DeleteBehavior.Restrict); // No borrar el producto si la categoría es borrada, para mantener el historial de productos de esa categoría.

            // --- PAGOS ---
            /// <summary>
            /// Si borramos una venta, normalmente si se deben borrar sus pagos con Cascade, porque no existe un pago sin venta.
            /// Pero por seguridad contable, se prefiere Restrict.
            /// </summary>
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Venta)
                .WithMany(v => v.Pagos)
                .HasForeignKey(p => p.IdVenta)
                .OnDelete(DeleteBehavior.Restrict); // No borrar el pago si la venta es borrada, para mantener el historial de pagos relacionados con esa venta.
        }
    }
}