using API.Data;
using API.DTO.Request.Informes;
using API.Services.Common;
using API.Services.Informes.Formatters;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Informes;

public class InformesExportService : IInformesExportService
{
    private readonly IInformesService _informesService;
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly CsvFormatter _csvFormatter;
    private readonly PdfFormatter _pdfFormatter;
    private readonly DocxFormatter _docxFormatter;
    private readonly ILogger<InformesExportService> _logger;

    public InformesExportService(
        IInformesService informesService,
        AppDbContext context,
        ICurrentUserService currentUser,
        CsvFormatter csvFormatter,
        PdfFormatter pdfFormatter,
        DocxFormatter docxFormatter,
        ILogger<InformesExportService> logger)
    {
        _informesService = informesService;
        _context = context;
        _currentUser = currentUser;
        _csvFormatter = csvFormatter;
        _pdfFormatter = pdfFormatter;
        _docxFormatter = docxFormatter;
        _logger = logger;
    }

    public async Task<ExportResult> ExportAsync(ExportInformesQuery query, CancellationToken ct)
    {
        var tipo = query.Tipo.ToLowerInvariant();
        var formato = query.Formato.ToLowerInvariant();
        var cantidad = query.Cantidad ?? 10;

        // Resolve periodo token for filename
        string periodo;
        if (query.FechaDesde.HasValue && query.FechaHasta.HasValue)
            periodo = $"{query.FechaDesde:yyyy-MM-dd}_{query.FechaHasta:yyyy-MM-dd}";
        else if (!string.IsNullOrWhiteSpace(query.Preset))
            periodo = query.Preset.ToLowerInvariant();
        else
            periodo = "mes";

        // Fetch business header
        var negocioId = _currentUser.NegocioId;
        var negocio = await _context.Negocios.FirstOrDefaultAsync(n => n.Id == negocioId, ct)
            ?? new Models.Negocio { Id = negocioId, Nombre = "Negocio", CUIT = "", Direccion = "" };

        _logger.LogInformation("Export informes tipo={Tipo} formato={Formato} periodo={Periodo} negocioId={NegocioId} userId={UserId}",
            tipo, formato, periodo, negocioId, _currentUser.UserId);

        try
        {
            // Fetch DTO reusing tenant-scoped service
            object dto = tipo switch
            {
                "ventas-resumen" => await _informesService.GetVentasResumenAsync(query.FechaDesde, query.FechaHasta, query.Preset, ct),
                "productos-top" => await _informesService.GetProductosTopAsync(cantidad, query.FechaDesde, query.FechaHasta, query.Preset, ct),
                "flujo-caja" => await _informesService.GetFlujoCajaAsync(query.FechaDesde, query.FechaHasta, query.Preset, ct),
                "ingresos-gastos" => await _informesService.GetIngresosGastosAsync(query.FechaDesde, query.FechaHasta, query.Preset, ct),
                "alertas-stock" => await _informesService.GetAlertasStockAsync(ct),
                "ventas-por-pago" => await _informesService.GetVentasPorPagoAsync(query.FechaDesde, query.FechaHasta, query.Preset, ct),
                "ventas-por-vendedor" => await _informesService.GetVentasPorVendedorAsync(query.FechaDesde, query.FechaHasta, query.Preset, ct),
                _ => throw new FluentValidation.ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure("tipo", $"Tipo debe ser uno de: ventas-resumen, productos-top, flujo-caja, ingresos-gastos, alertas-stock, ventas-por-pago, ventas-por-vendedor")
                })
            };

            return formato switch
            {
                "csv" => await _csvFormatter.FormatAsync(tipo, dto, periodo, negocio, ct),
                "pdf" => await _pdfFormatter.FormatAsync(tipo, dto, periodo, negocio, ct),
                "docx" => await _docxFormatter.FormatAsync(tipo, dto, periodo, negocio, ct),
                _ => throw new FluentValidation.ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure("formato", "Formato debe ser csv, pdf o docx.")
                })
            };
        }
        catch (FluentValidation.ValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export failed tipo={Tipo} formato={Formato} periodo={Periodo} negocioId={NegocioId}", tipo, formato, periodo, negocioId);
            throw;
        }
    }
}
