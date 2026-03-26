using FluentValidation;
using API.DTO.Request.Presupuestos;

namespace API.DTO.Request.Presupuestos
{
    public class UpdatePresupuestoValidator : AbstractValidator<UpdatePresupuestoRequest>
    {
        public UpdatePresupuestoValidator()
        {
            RuleFor(x => x.Estado)
                .IsInEnum().WithMessage("Estado inválido")
                .NotEmpty().WithMessage("El estado es requerido");
        }
    }
}
