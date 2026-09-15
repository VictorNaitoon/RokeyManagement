using FluentValidation;

namespace API.DTO.Request.Suscripcion
{
    public class SuscripcionRequestValidator : AbstractValidator<SuscripcionRequest>
    {
        public SuscripcionRequestValidator()
        {
            RuleFor(x => x.IdPlan)
                .GreaterThan(0).WithMessage("IdPlan es requerido y debe ser mayor a 0");

            RuleFor(x => x.TipoFacturacion)
                .IsInEnum().WithMessage("TipoFacturacion inválido. Use Mensual (1) o Anual (2)");
        }
    }
}
