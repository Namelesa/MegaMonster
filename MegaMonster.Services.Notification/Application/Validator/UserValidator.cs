using FluentValidation;
using MegaMonster.Services.Notification.Core.Models;

namespace MegaMonster.Services.Notification.Application.Validator;

public class UserValidator : AbstractValidator<UserDto>
{
    public UserValidator()
    {
        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(user => user.FirstName)
            .NotEmpty().WithMessage("FirstName cannot be empty.")
            .MinimumLength(3).WithMessage("FirstName must not be less than 3 characters.")
            .MaximumLength(25).WithMessage("FirstName must be at most 25 characters long.")
            .Matches(@"^[A-Za-zА-Яа-яЁё]+$").WithMessage("FirstName can only contain letters.");
        RuleFor(user => user.LastName)
            .NotEmpty().WithMessage("LastName cannot be empty.")
            .MinimumLength(3).WithMessage("LastName must not be less than 3 characters.")
            .MaximumLength(25).WithMessage("LastName must be at most 25 characters long.")
            .Matches(@"^[A-Za-zА-Яа-яЁё]+$").WithMessage("LastName can only contain letters.");
        RuleFor(user => user.Telegram)
            .NotEmpty().WithMessage("Telegram username cannot be empty.")
            .Matches(@"^@[A-Za-z0-9_-]{5,32}$").WithMessage("Telegram username must start with '@' and can only contain letters, numbers, '_' or '-'. Length should be between 5 and 32 characters.");
    }    
}