using FluentValidation;

namespace API.DTO.Request.Productos;

public class AjusteStockRequestValidator : AbstractValidator<AjusteStockRequest>
{
    public AjusteStockRequestValidator()
    {
        RuleFor(x => x.CantidadDelta).NotEqual(0).WithMessage("El delta de stock debe ser distinto de cero");
        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El motivo es requerido")
            .Must(m => !string.IsNullOrWhiteSpace(m) && m.Trim().Length >= 5)
                .WithMessage("El motivo debe tener al menos 5 caracteres")
            .Must(m => m == null || m.Trim().Length <= 500)
                .WithMessage("El motivo no puede exceder 500 caracteres");
    }
}