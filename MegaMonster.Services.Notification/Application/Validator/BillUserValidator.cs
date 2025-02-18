using FluentValidation;
using MegaMonster.Services.Notification.Core.Models;

namespace MegaMonster.Services.Notification.Application.Validator;

public class BillUserValidator : AbstractValidator<BillUserDto>
{
    public BillUserValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(user => user.UserName)
            .NotEmpty().WithMessage("FirstName cannot be empty.")
            .MinimumLength(3).WithMessage("FirstName must not be less than 3 characters.")
            .MaximumLength(25).WithMessage("FirstName must be at most 25 characters long.")
            .Matches(@"^[A-Za-zА-Яа-яЁё]+$").WithMessage("FirstName can only contain letters.");
    }
}