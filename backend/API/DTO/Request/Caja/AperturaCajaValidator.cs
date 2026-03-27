using FluentValidation;

namespace API.DTO.Request.Caja
{
    public class AperturaCajaValidator : AbstractValidator<AperturaCajaRequest>
    {
        public AperturaCajaValidator()
        {
            RuleFor(x => x.MontoInicial)
                .GreaterThanOrEqualTo(0)
                .WithMessage("El monto inicial debe ser mayor o igual a cero.");

            RuleFor(x => x.Observaciones)
                .MaximumLength(500)
                .WithMessage("Las observaciones no pueden exceder 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Observaciones));
        }
    }
}
