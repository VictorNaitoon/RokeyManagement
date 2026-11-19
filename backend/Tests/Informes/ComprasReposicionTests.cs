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
/// El informe de reposicion lista solo compras activas del negocio en el periodo.
/// </summary>
public class ComprasReposicionTests
{
    [Fact]
    public async Task GetComprasReposicionAsync_ShouldReturnOnlyActivePurchasesOfTenant()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        await using var context = new AppDbContext(options);

        context.Productos.AddRange(
            new Producto { Id = 1, Id_negocio = 1, Nombre = "Cerradura", PrecioCompra = 90m, PrecioVenta = 150m },
            new Producto { Id = 2, Id_negocio = 1, Nombre = "Llave", PrecioCompra = 10m, PrecioVenta = 30m });
        context.Proveedores.Add(new Proveedor { Id = 1, Id_negocio = 1, Nombre = "Proveedor Uno" });

        context.Compras.AddRange(
            new Compra
            {
                Id = 1, Id_negocio = 1, IdProveedor = 1, IdUsuario = 1,
                FechaCompra = new DateTime(2026, 1, 10, 10, 0, 0, DateTimeKind.Utc),
                TotalGasto = 350m, Anulada = false
            },
            new Compra
            {
                Id = 2, Id_negocio = 1, IdProveedor = 1, IdUsuario = 1,
                FechaCompra = new DateTime(2026, 1, 12, 10, 0, 0, DateTimeKind.Utc),
                TotalGasto = 999m, Anulada = true
            },
            new Compra
            {
                Id = 3, Id_negocio = 2, IdProveedor = 1, IdUsuario = 1,
                FechaCompra = new DateTime(2026, 1, 13, 10, 0, 0, DateTimeKind.Utc),
                TotalGasto = 888m, Anulada = false
            });
        context.DetallesCompra.AddRange(
            new DetalleCompra { Id = 1, IdCompra = 1, IdProducto = 1, Cantidad = 3, PrecioUnitario = 100m },
            new DetalleCompra { Id = 2, IdCompra = 1, IdProducto = 2, Cantidad = 5, PrecioUnitario = 10m },
            new DetalleCompra { Id = 3, IdCompra = 2, IdProducto = 1, Cantidad = 9, PrecioUnitario = 111m },
            new DetalleCompra { Id = 4, IdCompra = 3, IdProducto = 1, Cantidad = 8, PrecioUnitario = 111m });
        await context.SaveChangesAsync();

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(u => u.NegocioId).Returns(1);
        var service = new InformesService(context, currentUser.Object);

        var result = await service.GetComprasReposicionAsync(
            new DateTime(2026, 1, 1), new DateTime(2026, 1, 31), null, CancellationToken.None);

        Assert.Equal(350m, result.TotalInvertido);
        Assert.Equal(1, result.CantidadCompras);
        Assert.Equal(2, result.ProductosDistintos);
        Assert.Equal(2, result.Lineas.Count);
        Assert.All(result.Lineas, l => Assert.Equal("Proveedor Uno", l.Proveedor));
    }

    [Fact]
    public async Task CsvExport_ComprasReposicion_ShouldIncludeTotalsAndLines()
    {
        var dto = new ComprasReposicionResponse(
            TotalInvertido: 350m,
            CantidadCompras: 1,
            ProductosDistintos: 2,
            Lineas: new List<CompraReposicionItem>
            {
                new("Cerradura", 3, 100m, 300m, "Proveedor Uno", new DateTime(2026, 1, 10))
            });

        var result = await new CsvFormatter().FormatAsync(
            "compras-reposicion", dto, "mes", new Negocio { Id = 1, Nombre = "Prueba" }, CancellationToken.None);

        var csv = System.Text.Encoding.UTF8.GetString(result.Bytes);
        Assert.Contains("TotalInvertido", csv);
        Assert.Contains("350.00", csv);
        Assert.Contains("Cerradura", csv);
        Assert.Contains("Proveedor Uno", csv);
    }
}
