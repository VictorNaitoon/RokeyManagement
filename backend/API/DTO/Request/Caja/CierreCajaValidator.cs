using FluentValidation;

namespace API.DTO.Request.Caja
{
    public class CierreCajaValidator : AbstractValidator<CierreCajaRequest>
    {
        public CierreCajaValidator()
        {
            RuleFor(x => x.MontoFinal)
                .NotNull()
                .WithMessage("El monto final es obligatorio.")
                .GreaterThanOrEqualTo(0)
                .WithMessage("El monto final debe ser mayor o igual a cero.");

            RuleFor(x => x.Observaciones)
                .MaximumLength(500)
                .WithMessage("Las observaciones no pueden exceder 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Observaciones));
        }
    }
}
