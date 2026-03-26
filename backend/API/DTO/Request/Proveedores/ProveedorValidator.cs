using FluentValidation;

namespace API.DTO.Request.Proveedores
{
    public class CrearProveedorValidator : AbstractValidator<CrearProveedorRequest>
    {
        public CrearProveedorValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del proveedor es obligatorio.")
                .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

            RuleFor(x => x.Telefono)
                .MaximumLength(50).WithMessage("El teléfono no puede exceder 50 caracteres.");

            RuleFor(x => x.Email)
                .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres.")
                .EmailAddress().WithMessage("El formato del email no es válido.")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }

    public class ActualizarProveedorValidator : AbstractValidator<ActualizarProveedorRequest>
    {
        public ActualizarProveedorValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del proveedor es obligatorio.")
                .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres.");

            RuleFor(x => x.Telefono)
                .MaximumLength(50).WithMessage("El teléfono no puede exceder 50 caracteres.");

            RuleFor(x => x.Email)
                .MaximumLength(100).WithMessage("El email no puede exceder 100 caracteres.")
                .EmailAddress().WithMessage("El formato del email no es válido.")
                .When(x => !string.IsNullOrEmpty(x.Email));
        }
    }
}