using API.DTO.Request.Informes;

namespace API.Services.Informes;

public record ExportResult(byte[] Bytes, string ContentType, string FileName);

public interface IInformesExportService
{
    Task<ExportResult> ExportAsync(ExportInformesQuery query, CancellationToken ct);
}
