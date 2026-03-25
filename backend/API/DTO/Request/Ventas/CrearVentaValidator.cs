using FluentValidation;
using API.DTO.Request.Ventas;

namespace API.DTO.Request.Ventas
{
    public class CrearVentaValidator : AbstractValidator<CrearVentaRequest>
    {
        public CrearVentaValidator()
        {
            RuleFor(x => x.Detalles)
                .NotEmpty().WithMessage("La venta debe tener al menos un producto.");

            RuleForEach(x => x.Detalles).ChildRules(detalle =>
            {
                detalle.RuleFor(d => d.IdProducto)
                    .GreaterThan(0)
                    .WithMessage("El ID del producto debe ser mayor a 0.");

                detalle.RuleFor(d => d.Cantidad)
                    .GreaterThan(0)
                    .WithMessage("La cantidad debe ser mayor a cero.");

                detalle.RuleFor(d => d.PrecioUnitario)
                    .GreaterThan(0)
                    .WithMessage("El precio unitario debe ser mayor a cero.");
            });

            RuleFor(x => x.Pagos)
                .NotEmpty().WithMessage("Debe registrarse al menos un método de pago.");

            RuleForEach(x => x.Pagos).ChildRules(pago =>
            {
                pago.RuleFor(p => p.Monto)
                    .GreaterThan(0)
                    .WithMessage("El monto del pago debe ser mayor a cero.");
            });

            // Validar que la suma de pagos sea igual al total de la venta
            RuleFor(x => x)
                .Must((request) =>
                {
                    var totalVenta = request.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
                    var sumaPagos = request.Pagos.Sum(p => p.Monto);
                    return sumaPagos == totalVenta;
                })
                .WithMessage("La suma de pagos debe coincidir con el total de la venta.");
        }
    }
}
