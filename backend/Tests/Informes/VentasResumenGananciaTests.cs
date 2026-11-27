using System.Reflection;
using API.Data;
using API.DTO.Response.Informes;
using API.Models;
using API.Services.Common;
using API.Services.Informes;
using API.Services.Informes.Formatters;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Tests.Informes;

/// <summary>
/// Resumen de ventas debe exponer la ganancia calculada con el mismo margen por producto que la caja.
/// </summary>
public class VentasResumenGananciaTests
{
    [Fact]
    public async Task GetVentasResumenAsync_ShouldExposeGananciaUsingProductMargin()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AppDbContext(options);

        context.Productos.Add(new Producto
        {
            Id = 1,
            Id_negocio = 1,
            Nombre = "Cerradura",
            PrecioCompra = 100m,
            PrecioVenta = 200m,
            StockActual = 5,
            StockMinimo = 1,
            Activo = true
        });
        context.Ventas.Add(new Venta
        {
            Id = 1,
            Id_negocio = 1,
            IdUsuario = 1,
            FechaVenta = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            TotalVenta = 400m
        });
        context.DetallesVenta.Add(new DetalleVenta
        {
            Id = 1,
            IdVenta = 1,
            IdProducto = 1,
            Cantidad = 2,
            PrecioUnitario = 200m
        });
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.NegocioId).Returns(1);
        var service = new InformesService(context, currentUser.Object);

        var result = await service.GetVentasResumenAsync(
            new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), null, CancellationToken.None);

        // Total de ventas existente se mantiene igual.
        Assert.Equal(400m, result.TotalVentas);
        Assert.Equal(1, result.CantidadVentas);

        // Campo nuevo: ganancia = (precio venta - costo) * cantidad = (200 - 100) * 2.
        var prop = typeof(VentasResumenResponse).GetProperty("Ganancia");
        Assert.NotNull(prop);
        Assert.Equal(200m, Convert.ToDecimal(prop!.GetValue(result)));
    }

    [Fact]
    public async Task GetVentasResumenAsync_ShouldExposeCostoVendidoExcludingCancelledSales()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AppDbContext(options);

        context.Productos.Add(new Producto
        {
            Id = 1,
            Id_negocio = 1,
            Nombre = "Llave",
            PrecioCompra = 100m,
            PrecioVenta = 200m,
            StockActual = 10,
            StockMinimo = 1,
            Activo = true
        });
        context.Ventas.AddRange(
            new Venta
            {
                Id = 1,
                Id_negocio = 1,
                IdUsuario = 1,
                FechaVenta = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc),
                TotalVenta = 400m
            },
            new Venta
            {
                Id = 2,
                Id_negocio = 1,
                IdUsuario = 1,
                FechaVenta = new DateTime(2026, 1, 16, 12, 0, 0, DateTimeKind.Utc),
                TotalVenta = 600m,
                Anulada = true
            });
        context.DetallesVenta.AddRange(
            new DetalleVenta { Id = 1, IdVenta = 1, IdProducto = 1, Cantidad = 2, PrecioUnitario = 200m },
            new DetalleVenta { Id = 2, IdVenta = 2, IdProducto = 1, Cantidad = 3, PrecioUnitario = 200m });
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.NegocioId).Returns(1);
        var service = new InformesService(context, currentUser.Object);

        var result = await service.GetVentasResumenAsync(
            new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), null, CancellationToken.None);

        // La venta anulada no suma ni en ventas ni en costo.
        Assert.Equal(400m, result.TotalVentas);
        Assert.Equal(1, result.CantidadVentas);

        var costoProp = typeof(VentasResumenResponse).GetProperty("CostoVendido");
        Assert.NotNull(costoProp);
        Assert.Equal(200m, Convert.ToDecimal(costoProp!.GetValue(result)));
    }

    [Fact]
    public async Task CsvExport_VentasResumen_ShouldIncludeCostoVendido()
    {
        var dto = new VentasResumenResponse(
            TotalVentas: 1000m,
            CantidadVentas: 5,
            TicketPromedio: 200m,
            VentasAnuladas: 0,
            Periodo: "2026-01-01 al 2026-01-31",
            Ganancia: 350m,
            CostoVendido: 650m);

        var formatter = new CsvFormatter();
        var result = await formatter.FormatAsync(
            "ventas-resumen", dto, "mes", new Negocio { Id = 1, Nombre = "Prueba" }, CancellationToken.None);

        var csv = System.Text.Encoding.UTF8.GetString(result.Bytes);
        Assert.Contains("Ganancia", csv);
        Assert.Contains("350.00", csv);
        Assert.Contains("CostoVendido", csv);
        Assert.Contains("650.00", csv);
        Assert.DoesNotContain("VentasAnuladas", csv);
    }
}
