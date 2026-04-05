using FluentValidation;
using API.DTO.Request.Categorias;

namespace API.DTO.Request.Categorias
{
    public class CrearCategoriaRequestValidator : AbstractValidator<CrearCategoriaRequest>
    {
        public CrearCategoriaRequestValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido")
                .MaximumLength(100).WithMessage("El nombre no puede exceder 100 caracteres");

            RuleFor(x => x.Descripcion)
                .MaximumLength(500).WithMessage("La descripción no puede exceder 500 caracteres")
                .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
        }
    }
}