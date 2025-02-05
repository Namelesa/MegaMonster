using FluentValidation;
using MegaMonster.Services.User.Core.Models;

namespace MegaMonster.Services.User.Application.Validation.RoleValidator;

public class RoleValidation : AbstractValidator<Role>
{
    public RoleValidation()
    {
        RuleFor(role => role.RoleName)
            .Matches(@"^[A-Za-zА-Яа-яЁё]+$").WithMessage("RoleName can only contain letters.");
    }
}