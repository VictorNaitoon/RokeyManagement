using FluentValidation;
using API.DTO.Request.Facturas;

namespace API.DTO.Request.Facturas
{
    /// <summary>
    /// Validador para NotaCreditoRequest
    /// </summary>
    public class NotaCreditoValidator : AbstractValidator<NotaCreditoRequest>
    {
        public NotaCreditoValidator()
        {
            RuleFor(x => x.IdVenta)
                .GreaterThan(0)
                .WithMessage("El ID de la venta debe ser mayor a 0.");

            RuleFor(x => x.Motivo)
                .NotEmpty().WithMessage("El motivo es obligatorio.")
                .MaximumLength(500).WithMessage("El motivo no puede exceder 500 caracteres.");
        }
    }
}
