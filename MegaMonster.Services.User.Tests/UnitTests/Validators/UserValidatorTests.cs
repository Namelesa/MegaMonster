using FluentValidation;
using FluentValidation.TestHelper;
using MegaMonster.Services.User.Application.User;
using MegaMonster.Services.User.Core.User;

namespace MegaMonster.Services.User.Tests.UnitTests.Validators;

public class UserValidatorTests
{
    private readonly IValidator<Users> _validator;

    public UserValidatorTests()
    {
        _validator = new UserValidation();
    }

    [Fact]
    public void Should_Have_Error_When_UserName_Is_Empty()
    {
        // Arrange
        var user = new Users("ValidLogin")
        {
            UserName = "",
            Email = "valid@example.com",
            PhoneNumber = "+1234567890"
        };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.UserName)
            .WithErrorMessage("Username cannot be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var user = new Users("ValidLogin")
        {
            UserName = "ValidUser",
            Email = "",
            PhoneNumber = "+1234567890"
        };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email cannot be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Is_Empty()
    {
        // Arrange
        var user = new Users("ValidLogin")
        {
            UserName = "ValidUser",
            Email = "valid@example.com",
            PhoneNumber = ""
        };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("'Phone Number' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Is_Invalid_Format()
    {
        // Arrange
        var user = new Users("ValidLogin")
        {
            UserName = "ValidUser",
            Email = "valid@example.com",
            PhoneNumber = "12345"
        };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber)
            .WithErrorMessage("PhoneNumber not valid");
    }

    [Fact]
    public void Should_Pass_When_All_Fields_Are_Valid()
    {
        // Arrange
        var user = new Users("ValidLogin")
        {
            UserName = "ValidUsers",
            Email = "valid@example.com",
            PhoneNumber = "+1234567890"
        };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }
}