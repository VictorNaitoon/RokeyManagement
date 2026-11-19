using System.Reflection;
using API.Data;
using API.DTO.Response.Informes;
using API.Models;
using API.Services.Common;
using API.Services.Informes;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace API.Tests.Informes;

/// <summary>
/// Una compra grande de reposición no debe convertir la ganancia en negativa:
/// ganancia = margen de lo vendido; flujo neto = ventas - compras.
/// </summary>
public class IngresosGastosGananciaTests
{
    [Fact]
    public async Task GetIngresosGastosAsync_ShouldKeepMarginAsGananciaAndSeparateFlujoNeto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AppDbContext(options);

        context.Productos.Add(new Producto
        {
            Id = 1, Id_negocio = 1, Nombre = "Cerradura",
            PrecioCompra = 100m, PrecioVenta = 200m, StockActual = 20, StockMinimo = 2
        });
        context.Ventas.Add(new Venta
        {
            Id = 1, Id_negocio = 1, IdUsuario = 1,
            FechaVenta = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            TotalVenta = 400m
        });
        context.DetallesVenta.Add(new DetalleVenta
        {
            Id = 1, IdVenta = 1, IdProducto = 1, Cantidad = 2, PrecioUnitario = 200m
        });
        context.Compras.Add(new Compra
        {
            Id = 1, Id_negocio = 1, IdProveedor = 1, IdUsuario = 1,
            FechaCompra = new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc),
            TotalGasto = 1000m
        });
        context.DetallesCompra.Add(new DetalleCompra
        {
            Id = 1, IdCompra = 1, IdProducto = 1, Cantidad = 10, PrecioUnitario = 100m
        });
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.NegocioId).Returns(1);
        var service = new InformesService(context, currentUser.Object);

        var result = await service.GetIngresosGastosAsync(
            new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), null, CancellationToken.None);

        Assert.Equal(400m, result.VentasTotales);
        Assert.Equal(1000m, result.ComprasTotales);

        var gananciaProp = typeof(IngresosGastosResponse).GetProperty("Ganancia");
        Assert.NotNull(gananciaProp);
        Assert.Equal(200m, Convert.ToDecimal(gananciaProp!.GetValue(result)));

        var flujoNetoProp = typeof(IngresosGastosResponse).GetProperty("FlujoNeto");
        Assert.NotNull(flujoNetoProp);
        Assert.Equal(-600m, Convert.ToDecimal(flujoNetoProp!.GetValue(result)));

        // Compatibilidad: el campo viejo conserva el valor del flujo neto.
        Assert.Equal(-600m, result.GananciaBruta);

        // Margen calculado sobre la ganancia real, no sobre el flujo neto.
        Assert.Equal(50m, result.MargenPorcentaje);
    }
}
