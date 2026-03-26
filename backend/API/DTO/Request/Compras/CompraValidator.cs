using FluentValidation;

namespace API.DTO.Request.Compras
{
    public class CrearCompraValidator : AbstractValidator<CrearCompraRequest>
    {
        public CrearCompraValidator()
        {
            RuleFor(x => x.NumeroComprobante)
                .NotEmpty().WithMessage("El número de comprobante es obligatorio.")
                .MaximumLength(50).WithMessage("El número de comprobante no puede exceder 50 caracteres.");

            RuleFor(x => x.IdProveedor)
                .GreaterThan(0).WithMessage("El proveedor es obligatorio.");

            RuleFor(x => x.Detalles)
                .NotEmpty().WithMessage("La compra debe tener al menos un producto.");

            RuleForEach(x => x.Detalles).ChildRules(detalle =>
            {
                detalle.RuleFor(d => d.IdProducto)
                    .GreaterThan(0).WithMessage("El producto es obligatorio.");

                detalle.RuleFor(d => d.Cantidad)
                    .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero.");

                detalle.RuleFor(d => d.PrecioUnitario)
                    .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a cero.");
            });
        }
    }

    public class AnularCompraValidator : AbstractValidator<AnularCompraRequest>
    {
        public AnularCompraValidator()
        {
            RuleFor(x => x.Motivo)
                .MaximumLength(500).WithMessage("El motivo no puede exceder 500 caracteres.");
        }
    }
}