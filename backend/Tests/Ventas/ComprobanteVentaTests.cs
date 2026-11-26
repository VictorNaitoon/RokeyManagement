using API.Data;
using API.Models;
using API.Services.Auditoria;
using API.Services.Caja;
using API.Services.Common;
using API.Services.Ventas;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace API.Tests.Ventas;

/// <summary>
/// El comprobante de venta debe generarse en PDF y PNG para imprimir o compartir,
/// siempre dentro del negocio autenticado.
/// </summary>
public class ComprobanteVentaTests
{
    private static async Task<(AppDbContext Context, ComprobanteVentaService Service)> CrearEscenarioAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);

        context.Negocios.Add(new Negocio
        {
            Id = 1,
            Nombre = "Cerrajería Don José",
            Direccion = "Av. Siempre Viva 123",
            Telefono = "11 5555-5555"
        });
        context.Usuarios.Add(new Usuario
        {
            Id = 1,
            Id_negocio = 1,
            Nombre = "José",
            Apellido = "Pérez",
            Email = "jose@test.com",
            PasswordHash = "hash"
        });
        context.Productos.Add(new Producto
        {
            Id = 1,
            Id_negocio = 1,
            Nombre = "Llave de cerradura",
            PrecioCompra = 100m,
            PrecioVenta = 450m,
            StockActual = 10,
            StockMinimo = 1,
            Activo = true
        });
        context.Clientes.Add(new Cliente { Id = 1, Id_negocio = 1, Nombre = "Consumidor Final" });
        context.Ventas.AddRange(
            new Venta
            {
                Id = 10,
                Id_negocio = 1,
                IdUsuario = 1,
                IdCliente = 1,
                FechaVenta = new DateTime(2026, 11, 26, 15, 0, 0, DateTimeKind.Utc),
                TotalVenta = 900m
            },
            new Venta
            {
                Id = 99,
                Id_negocio = 2,
                IdUsuario = 1,
                IdCliente = 1,
                FechaVenta = new DateTime(2026, 11, 26, 15, 0, 0, DateTimeKind.Utc),
                TotalVenta = 900m
            });
        context.DetallesVenta.AddRange(
            new DetalleVenta
            {
                Id = 1,
                IdVenta = 10,
                IdProducto = 1,
                Cantidad = 2,
                PrecioUnitario = 450m
            },
            new DetalleVenta
            {
                Id = 2,
                IdVenta = 99,
                IdProducto = 1,
                Cantidad = 2,
                PrecioUnitario = 450m
            });
        context.Pagos.AddRange(
            new Pago { Id = 1, IdVenta = 10, MetodoPago = Enums.MetodoPago.Efectivo, Monto = 900m },
            new Pago { Id = 2, IdVenta = 99, MetodoPago = Enums.MetodoPago.Efectivo, Monto = 900m });
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.NegocioId).Returns(1);
        currentUser.SetupGet(u => u.UserId).Returns(1);

        var ventaService = new VentaService(
            context,
            currentUser.Object,
            Mock.Of<ICajaService>(),
            Mock.Of<IAuditoriaService>(),
            NullLogger<VentaService>.Instance);

        var service = new ComprobanteVentaService(ventaService, context, currentUser.Object);
        return (context, service);
    }

    [Fact]
    public async Task GenerarAsync_Pdf_DevuelveUnPdfDelTicket()
    {
        var (context, service) = await CrearEscenarioAsync();
        await using var _ = context;

        var file = await service.GenerarAsync(10, "pdf", CancellationToken.None);

        Assert.NotNull(file);
        Assert.Equal("application/pdf", file!.ContentType);
        Assert.Equal("venta-10.pdf", file.FileName);
        Assert.True(file.Content.Length > 1000, $"El PDF pesa {file.Content.Length} bytes.");
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(file.Content, 0, 4));
    }

    [Fact]
    public async Task GenerarAsync_Png_DevuelveUnaImagenPngDelTicket()
    {
        var (context, service) = await CrearEscenarioAsync();
        await using var _ = context;

        var file = await service.GenerarAsync(10, "png", CancellationToken.None);

        Assert.NotNull(file);
        Assert.Equal("image/png", file!.ContentType);
        Assert.Equal("venta-10.png", file.FileName);
        var firmaPng = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
        Assert.Equal(firmaPng, file.Content.Take(4));
    }

    [Fact]
    public async Task GenerarAsync_VentaDeOtroNegocio_DevuelveNull()
    {
        var (context, service) = await CrearEscenarioAsync();
        await using var _ = context;

        var file = await service.GenerarAsync(99, "pdf", CancellationToken.None);

        Assert.Null(file);
    }

    [Fact]
    public async Task GenerarAsync_FormatoInvalido_LanzaValidacion()
    {
        var (context, service) = await CrearEscenarioAsync();
        await using var _ = context;

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.GenerarAsync(10, "jpg", CancellationToken.None));
    }
}
