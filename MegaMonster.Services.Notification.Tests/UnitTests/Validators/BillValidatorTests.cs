using FluentValidation;
using FluentValidation.TestHelper;
using MegaMonster.Services.Notification.Application.Validator;
using MegaMonster.Services.Notification.Core.User;

namespace MegaMonster.Services.Notification.Tests.UnitTests.Validators;

public class BillUserValidatorTests
{
    private readonly IValidator<BillUserDto> _validator;

    public BillUserValidatorTests()
    {
        _validator = new BillUserValidator();
    }

    #region Email Validation Tests

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var user = new BillUserDto { Email = "", UserName = "ValidUser", OrderId = Guid.NewGuid(), PaymentType = "Credit", Status = "Paid", Sum = 100.0, OrderDetailsRows = new List<int>() };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Email cannot be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Invalid_Format()
    {
        // Arrange
        var user = new BillUserDto { Email = "invalid-email", UserName = "ValidUser", OrderId = Guid.NewGuid(), PaymentType = "Credit", Status = "Paid", Sum = 100.0, OrderDetailsRows = new List<int>() };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("Invalid email format.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Email_Is_Valid()
    {
        // Arrange
        var user = new BillUserDto { Email = "valid@example.com", UserName = "ValidUser", OrderId = Guid.NewGuid(), PaymentType = "Credit", Status = "Paid", Sum = 100.0, OrderDetailsRows = new List<int>() };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region UserName Validation Tests

    [Fact]
    public void Should_Have_Error_When_UserName_Is_Empty()
    {
        // Arrange
        var user = new BillUserDto { Email = "valid@example.com", UserName = "", OrderId = Guid.NewGuid(), PaymentType = "Credit", Status = "Paid", Sum = 100.0, OrderDetailsRows = new List<int>() };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage("FirstName cannot be empty.");
    }

    [Fact]
    public void Should_Have_Error_When_UserName_Is_Less_Than_Min_Length()
    {
        // Arrange
        var user = new BillUserDto { Email = "valid@example.com", UserName = "ab", OrderId = Guid.NewGuid(), PaymentType = "Credit", Status = "Paid", Sum = 100.0, OrderDetailsRows = new List<int>() };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage("FirstName must not be less than 3 characters.");
    }

    [Fact]
    public void Should_Have_Error_When_UserName_Is_Greater_Than_Max_Length()
    {
        // Arrange
        var user = new BillUserDto { Email = "valid@example.com", UserName = new string('a', 26), OrderId = Guid.NewGuid(), PaymentType = "Credit", Status = "Paid", Sum = 100.0, OrderDetailsRows = new List<int>() };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage("FirstName must be at most 25 characters long.");
    }

    [Fact]
    public void Should_Have_Error_When_UserName_Contains_Invalid_Characters()
    {
        // Arrange
        var user = new BillUserDto { Email = "valid@example.com", UserName = "Invalid123", OrderId = Guid.NewGuid(), PaymentType = "Credit", Status = "Paid", Sum = 100.0, OrderDetailsRows = new List<int>() };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldHaveValidationErrorFor(x => x.UserName).WithErrorMessage("FirstName can only contain letters.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_UserName_Is_Valid()
    {
        // Arrange
        var user = new BillUserDto { Email = "valid@example.com", UserName = "ValidUser", OrderId = Guid.NewGuid(), PaymentType = "Credit", Status = "Paid", Sum = 100.0, OrderDetailsRows = new List<int>() };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    #endregion

    #region Full Valid User Tests

    [Fact]
    public void Should_Not_Have_Error_When_UserName_And_Email_Are_Valid()
    {
        // Arrange
        var user = new BillUserDto
        {
            Email = "valid@example.com",
            UserName = "ValidUser",
            OrderId = Guid.NewGuid(),
            PaymentType = "Credit",
            Status = "Paid",
            Sum = 100.0,
            OrderDetailsRows = new List<int>()
        };

        // Act & Assert
        var result = _validator.TestValidate(user);
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
        result.ShouldNotHaveValidationErrorFor(x => x.UserName);
    }

    #endregion
}