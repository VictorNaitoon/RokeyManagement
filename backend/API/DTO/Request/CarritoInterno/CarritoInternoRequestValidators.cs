using FluentValidation;

namespace API.DTO.Request.CarritoInterno
{
    public class CreateCarritoInternoRequestValidator : AbstractValidator<CreateCarritoInternoRequest>
    {
        public CreateCarritoInternoRequestValidator()
        {
            RuleFor(x => x.Nombre)
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");
        }
    }

    public class AgregarItemRequestValidator : AbstractValidator<AgregarItemRequest>
    {
        public AgregarItemRequestValidator()
        {
            RuleFor(x => x.IdProducto)
                .GreaterThan(0).WithMessage("El ID del producto es requerido");

            RuleFor(x => x.Cantidad)
                .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero");

            RuleFor(x => x.Notas)
                .MaximumLength(500).WithMessage("Las notas no pueden exceder 500 caracteres");
        }
    }

    public class UpdateItemRequestValidator : AbstractValidator<UpdateItemRequest>
    {
        public UpdateItemRequestValidator()
        {
            RuleFor(x => x.Cantidad)
                .GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser negativa");
        }
    }

    public class ConvertirCarritoRequestValidator : AbstractValidator<ConvertirCarritoRequest>
    {
        public ConvertirCarritoRequestValidator()
        {
            RuleFor(x => x.IdCaja)
                .GreaterThan(0).WithMessage("El ID de la caja es requerido");

            RuleFor(x => x.FormaPago)
                .IsInEnum().WithMessage("Método de pago inválido");
        }
    }
}
