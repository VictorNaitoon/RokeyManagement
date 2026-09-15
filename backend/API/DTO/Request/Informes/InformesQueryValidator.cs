using FluentValidation;

namespace API.DTO.Request.Informes;

public record InformesQuery(
    string? Preset,
    DateTime? FechaDesde,
    DateTime? FechaHasta,
    int? Cantidad
);

public class InformesQueryValidator : AbstractValidator<InformesQuery>
{
    private static readonly string[] PresetsValidos = ["hoy", "semana", "mes"];

    public InformesQueryValidator()
    {
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
            .Must(c => c == null || (c >= 1 && c <= 50))
            .WithMessage("cantidad debe estar entre 1 y 50.")
            .When(x => x.Cantidad.HasValue);
    }
}
