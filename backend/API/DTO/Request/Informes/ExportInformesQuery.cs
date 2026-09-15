namespace API.DTO.Request.Informes;

public record ExportInformesQuery(
    string Tipo,
    string Formato,
    string? Preset,
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    int? Cantidad
);
