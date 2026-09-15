using FluentValidation;

namespace API.DTO.Request.Informes;

public class ExportInformesQueryValidator : AbstractValidator<ExportInformesQuery>
{
    private static readonly string[] TiposValidos =
    [
        "ventas-resumen", "productos-top", "flujo-caja", "ingresos-gastos",
        "alertas-stock", "ventas-por-pago", "ventas-por-vendedor"
    ];

    private static readonly string[] FormatosValidos = ["csv", "pdf", "docx"];
    private static readonly string[] PresetsValidos = ["hoy", "semana", "mes"];

    public ExportInformesQueryValidator()
    {
        RuleFor(x => x.Tipo)
            .Must(t => t != null && TiposValidos.Contains(t.ToLowerInvariant()))
            .WithMessage($"Tipo debe ser uno de: {string.Join(", ", TiposValidos)}");

        RuleFor(x => x.Formato)
            .NotEmpty().WithMessage("Formato es requerido.")
            .Must(f => FormatosValidos.Contains(f.ToLowerInvariant()))
            .WithMessage("Formato debe ser csv, pdf o docx.");

        RuleFor(x => x.Preset)
            .Must(p => p == null || PresetsValidos.Contains(p.ToLowerInvariant()))
            .WithMessage("Preset debe ser hoy, semana o mes.")
            .When(x => x.Preset != null);

        RuleFor(x => x)
            .Must(q => !(q.FechaDesde.HasValue ^ q.FechaHasta.HasValue))
            .WithMessage("fechaDesde y fechaHasta deben proporcionarse juntas.")
            .OverridePropertyName("fechaDesde");

        RuleFor(x => x)
            .Must(q => !q.FechaDesde.HasValue || q.FechaDesde <= q.FechaHasta)
            .WithMessage("fechaDesde no puede ser posterior a fechaHasta.")
            .OverridePropertyName("fechaDesde");

        RuleFor(x => x)
            .Must(q => !q.FechaDesde.HasValue || (q.FechaHasta!.Value - q.FechaDesde.Value).TotalDays <= 365)
            .WithMessage("El rango no puede exceder 12 meses.")
            .OverridePropertyName("fechaHasta");

        RuleFor(x => x.Cantidad)
            .Must((q, c) => q.Tipo.ToLowerInvariant() == "productos-top" || c == null)
            .WithMessage("cantidad solo es válido para tipo productos-top.")
            .Must(c => c == null || (c >= 1 && c <= 50))
            .WithMessage("cantidad debe estar entre 1 y 50.");
    }
}
