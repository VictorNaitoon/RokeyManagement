using FluentValidation;

namespace API.DTO.Request.Productos;

public class AjusteStockRequestValidator : AbstractValidator<AjusteStockRequest>
{
    public AjusteStockRequestValidator()
    {
        RuleFor(x => x.CantidadDelta).NotEqual(0).WithMessage("El delta de stock debe ser distinto de cero");
        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("El motivo es requerido")
            .MinimumLength(5).WithMessage("El motivo debe tener al menos 5 caracteres")
            .MaximumLength(500).WithMessage("El motivo no puede exceder 500 caracteres");
    }
}