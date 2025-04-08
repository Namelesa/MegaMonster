using FluentValidation;
using FluentValidation.TestHelper;
using MegaMonster.Services.Auth.Application.User;
using MegaMonster.Services.Auth.Core.User;

namespace MegaMonster.Services.Auth.Tests.UnitTests.Validator;

public class UserValidatorTests
{
    private readonly IValidator<Users> _validator;

    public UserValidatorTests()
    {
        _validator = new UserValidator();
    }

    #region UserName Validation Tests

    [Fact]
    public void Should_Have_Error_When_UserName_Is_Empty()
    {
        // Arrange
        var user = new Users { UserName = "", Email = "valid@example.com", Login = "ValidLogin1", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage("Username cannot be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_UserName_Is_Less_Than_Min_Length()
    {
        // Arrange
        var user = new Users { UserName = "Short", Email = "valid@example.com", Login = "ValidLogin1", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage("UserName must not be less than 10 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_UserName_Is_Greater_Than_Max_Length()
    {
        // Arrange
        var user = new Users { UserName = new string('a', 26), Email = "valid@example.com", Login = "ValidLogin1", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage("Username must be at most 25 characters long.");
    }

    [Fact]
    public void Should_Have_Error_When_UserName_Contains_Non_Letter_Characters()
    {
        // Arrange
        var user = new Users { UserName = "Invalid123", Email = "valid@example.com", Login = "ValidLogin1", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage("Username can only contain letters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_UserName_Is_Valid()
    {
        // Arrange
        var user = new Users { UserName = "ValidUsert", Email = "valid@example.com", Login = "ValidLogin1", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    #endregion

    #region Email Validation Tests

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var user = new Users { Email = "", UserName = "ValidUser", Login = "ValidLogin1", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email cannot be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid_Format()
    {
        // Arrange
        var user = new Users { Email = "invalid-email", UserName = "ValidUser", Login = "ValidLogin1", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email format.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Email_Is_Valid()
    {
        // Arrange
        var user = new Users { Email = "valid@example.com", UserName = "ValidUser", Login = "ValidLogin1", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Login Validation Tests

    [Fact]
    public void Should_Have_Error_When_Login_Is_Empty()
    {
        // Arrange
        var user = new Users { Login = "", UserName = "ValidUser", Email = "valid@example.com", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.Login).WithErrorMessage("Login cannot be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_Login_Is_Less_Than_Min_Length()
    {
        // Arrange
        var user = new Users { Login = "usr", UserName = "ValidUser", Email = "valid@example.com", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.Login).WithErrorMessage("Login must not be less than 5 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_Login_Is_Greater_Than_Max_Length()
    {
        // Arrange
        var user = new Users { Login = new string('a', 21), UserName = "ValidUser", Email = "valid@example.com", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.Login).WithErrorMessage("Login must not exceed 20 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_Login_Does_Not_Contain_Uppercase_Lowercase_And_Number()
    {
        // Arrange
        var user = new Users { Login = "invalidlogin", UserName = "ValidUser", Email = "valid@example.com", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.Login).WithErrorMessage("Login must contain at least one uppercase letter, one lowercase letter, and one digit.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Login_Is_Valid()
    {
        // Arrange
        var user = new Users { Login = "ValidLogin1", UserName = "ValidUser", Email = "valid@example.com", PhoneNumber = "+1234567890" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldNotHaveValidationErrorFor(x => x.Login);
    }

    #endregion

    #region PhoneNumber Validation Tests

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Is_Empty()
    {
        // Arrange
        var user = new Users { PhoneNumber = "", UserName = "ValidUser", Email = "valid@example.com", Login = "ValidLogin1" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber).WithErrorMessage("'Phone Number' must not be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_PhoneNumber_Is_Invalid_Format()
    {
        // Arrange
        var user = new Users { PhoneNumber = "12345", UserName = "ValidUser", Email = "valid@example.com", Login = "ValidLogin1" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber).WithErrorMessage("PhoneNumber not valid");
    }

    [Fact]
    public void Should_Not_Have_Error_When_PhoneNumber_Is_Valid()
    {
        // Arrange
        var user = new Users { PhoneNumber = "+1234567890", UserName = "ValidUser", Email = "valid@example.com", Login = "ValidLogin1" };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    #endregion

    #region Full Valid User Tests

    [Fact]
    public void Should_Not_Have_Error_When_User_Is_Valid()
    {
        // Arrange
        var user = new Users
        {
            UserName = "ValidUsert",
            Email = "valid@example.com",
            Login = "ValidLogin1",
            PhoneNumber = "+1234567890"
        };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.Login);
        result.ShouldNotHaveValidationErrorFor(x => x.PhoneNumber);
    }

    #endregion
}