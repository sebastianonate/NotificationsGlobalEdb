using FluentValidation;
using Notifications.Infrastructure.Teams;

namespace Notifications.WebAPI.Validators
{
    public class TeamsNotificationDataValidator: AbstractValidator<TeamsNotificationData>
    {
        public TeamsNotificationDataValidator()
        {
            RuleFor(x => x.SolutionId).Must(x => x != default(Guid) && x != Guid.Empty).WithMessage("Es requerido. No debe estar vacio");
            RuleFor(x => x.Message).Must(x => !string.IsNullOrWhiteSpace(x)).WithMessage("Es requerido. No debe estar vacio");
            RuleFor(x => x.To).Must(BeNotNullOrEmpty).WithMessage("Es requerido. No debe estar vacio");
        }

        private bool BeNotNullOrEmpty<T>(List<T> lista)
        {
            if (lista != null)
            {
                return lista.Any();
            }
            return false;
        }
    }
}
