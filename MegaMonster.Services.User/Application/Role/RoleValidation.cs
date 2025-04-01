using FluentValidation;

namespace MegaMonster.Services.User.Application.Role;

public class RoleValidation : AbstractValidator<Core.Role.Role>
{
    public RoleValidation()
    {
        RuleFor(role => role.RoleName)
            .Matches(@"^[A-Za-zА-Яа-яЁё]+$").WithMessage("RoleName can only contain letters.");
    }
}