using FluentValidation;

namespace API.DTO.Request.Caja
{
    public class MovimientoCajaValidator : AbstractValidator<AgregarMovimientoCajaRequest>
    {
        public MovimientoCajaValidator()
        {
            RuleFor(x => x.Tipo)
                .NotEmpty().WithMessage("El tipo de movimiento es obligatorio.")
                .Must(tipo => tipo == "Ingreso" || tipo == "Egreso")
                .WithMessage("El tipo debe ser 'Ingreso' o 'Egreso'.");

            RuleFor(x => x.Monto)
                .GreaterThan(0)
                .WithMessage("El monto debe ser mayor a cero.");

            RuleFor(x => x.Descripcion)
                .MaximumLength(500)
                .WithMessage("La descripción no puede exceder 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Descripcion));
        }
    }
}
