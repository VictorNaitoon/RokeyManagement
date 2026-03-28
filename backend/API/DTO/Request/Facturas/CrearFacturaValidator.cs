using FluentValidation;
using API.DTO.Request.Facturas;

namespace API.DTO.Request.Facturas
{
    /// <summary>
    /// Validador para CrearFacturaRequest
    /// </summary>
    public class CrearFacturaValidator : AbstractValidator<CrearFacturaRequest>
    {
        public CrearFacturaValidator()
        {
            RuleFor(x => x.IdVenta)
                .GreaterThan(0)
                .WithMessage("El ID de la venta debe ser mayor a 0.");

            RuleFor(x => x.CUIT_cliente)
                .NotEmpty().WithMessage("El CUIT del cliente es obligatorio.")
                .MaximumLength(20).WithMessage("El CUIT no puede exceder 20 caracteres.");

            RuleFor(x => x.TipoComprobante)
                .IsInEnum().WithMessage("El tipo de comprobante no es válido.");

            RuleFor(x => x.CondicionVenta)
                .NotEmpty().WithMessage("La condición de venta es obligatoria.")
                .MaximumLength(100).WithMessage("La condición de venta no puede exceder 100 caracteres.");
        }
    }
}
