using System.Text.RegularExpressions;
using FluentValidation;
using MegaMonster.Services.User.Core.Models;

namespace MegaMonster.Services.User.Application.Validation.UserValidator;

public class UserValidation : AbstractValidator<Users>
{
    public UserValidation()
    {
        RuleFor(users => users.UserName)
            .NotEmpty().WithMessage("Username cannot be empty.")
            .MinimumLength(10).WithMessage("UserName must not be less than 10 characters.")
            .MaximumLength(25).WithMessage("Username must be at most 25 characters long.")
            .Matches(@"^[A-Za-zА-Яа-яЁё]+$").WithMessage("Username can only contain letters.");
        RuleFor(users => users.Email)
            .NotEmpty().WithMessage("Email cannot be empty.")
            .EmailAddress().WithMessage("Invalid email format.");
        RuleFor(p => p.PhoneNumber)
            .NotEmpty()
            .NotNull().WithMessage("Phone Number is required.")
            .MinimumLength(10).WithMessage("PhoneNumber must not be less than 10 characters.")
            .MaximumLength(20).WithMessage("PhoneNumber must not exceed 20 characters.")
            .Matches(new Regex(@"^\+?\d{1,4}(\s?\(?\d{1,4}\)?[\s\-]?)?[\d\s\-]{7,15}$")).WithMessage("PhoneNumber not valid");
    }
}