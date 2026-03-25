using FluentValidation;
using API.DTO.Request.Productos;

namespace API.DTO.Request.Productos
{
    public class ActualizarProductoRequestValidator : AbstractValidator<ActualizarProductoRequest>
    {
        public ActualizarProductoRequestValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

            RuleFor(x => x.CodigoBusqueda)
                .MaximumLength(50).WithMessage("El código de búsqueda no puede exceder 50 caracteres")
                .When(x => !string.IsNullOrWhiteSpace(x.CodigoBusqueda));

            RuleFor(x => x.Descripcion)
                .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
                .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));

            RuleFor(x => x.PrecioVenta)
                .GreaterThan(0).WithMessage("El precio de venta debe ser mayor a 0");

            RuleFor(x => x.PrecioCompra)
                .GreaterThanOrEqualTo(0).WithMessage("El precio de compra no puede ser negativo")
                .When(x => x.PrecioCompra.HasValue);

            RuleFor(x => x.StockActual)
                .GreaterThanOrEqualTo(0).WithMessage("El stock actual no puede ser negativo");

            RuleFor(x => x.StockMinimo)
                .GreaterThanOrEqualTo(0).WithMessage("El stock mínimo no puede ser negativo");
        }
    }
}