using FluentValidation;
using API.DTO.Request.Presupuestos;

namespace API.DTO.Request.Presupuestos
{
    public class CreatePresupuestoValidator : AbstractValidator<CreatePresupuestoRequest>
    {
        public CreatePresupuestoValidator()
        {
            RuleFor(x => x.Detalles)
                .NotEmpty().WithMessage("El presupuesto debe tener al menos un producto.");

            RuleFor(x => x.FechaVencimiento)
                .GreaterThan(DateTime.Now.Date)
                .WithMessage("La fecha de vencimiento debe ser futura.")
                .When(x => x.FechaVencimiento.HasValue);

            RuleForEach(x => x.Detalles).ChildRules(detalle =>
            {
                detalle.RuleFor(d => d.IdProducto)
                    .GreaterThan(0)
                    .WithMessage("El ID del producto debe ser mayor a 0.");

                detalle.RuleFor(d => d.Cantidad)
                    .GreaterThan(0)
                    .WithMessage("La cantidad debe ser mayor a cero.");

                detalle.RuleFor(d => d.PrecioPactado)
                    .GreaterThan(0)
                    .WithMessage("El precio pactado debe ser mayor a cero.");
            });
        }
    }
}
