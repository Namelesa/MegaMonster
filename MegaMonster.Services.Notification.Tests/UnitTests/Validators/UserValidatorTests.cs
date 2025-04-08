using FluentValidation.TestHelper;
using MegaMonster.Services.Notification.Application.Validator;
using MegaMonster.Services.Notification.Core.User;

namespace MegaMonster.Services.Notification.Tests.UnitTests.Validators
{
    public class UserValidatorTests
    {
        private readonly UserValidator _validator;

        public UserValidatorTests()
        {
            _validator = new UserValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Empty()
        {
            // Arrange
            var user = new UserDto("TestName", "");

            // Act & Assert
            var result = _validator.TestValidate(user);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                .WithErrorMessage("'Email' must not be empty.");
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            // Arrange
            var user = new UserDto("username", "invalidemail");

            // Act & Assert
            var result = _validator.TestValidate(user);
            result.ShouldHaveValidationErrorFor(x => x.Email)
                  .WithErrorMessage("Invalid email format.");
        }

        [Fact]
        public void Should_Have_Error_When_UserName_Is_Empty()
        {
            // Arrange
            var user = new UserDto("", "validemail@example.com");

            // Act & Assert
            var result = _validator.TestValidate(user);
            result.ShouldHaveValidationErrorFor(x => x.UserName)
                  .WithErrorMessage("'User Name' must not be empty.");
        }

        [Fact]
        public void Should_Have_Error_When_UserName_Is_Too_Short()
        {
            // Arrange
            var user = new UserDto("ab", "validemail@example.com");

            // Act & Assert
            var result = _validator.TestValidate(user);
            result.ShouldHaveValidationErrorFor(x => x.UserName)
                  .WithErrorMessage("FirstName must not be less than 3 characters.");
        }

        [Fact]
        public void Should_Have_Error_When_UserName_Is_Too_Long()
        {
            // Arrange
            var user = new UserDto(new string('a', 26), "validemail@example.com");

            // Act & Assert
            var result = _validator.TestValidate(user);
            result.ShouldHaveValidationErrorFor(x => x.UserName)
                  .WithErrorMessage("FirstName must be at most 25 characters long.");
        }

        [Fact]
        public void Should_Have_Error_When_UserName_Has_Invalid_Characters()
        {
            // Arrange
            var user = new UserDto("User123", "validemail@example.com");

            // Act & Assert
            var result = _validator.TestValidate(user);
            result.ShouldHaveValidationErrorFor(x => x.UserName)
                  .WithErrorMessage("UserName can only contain letters.");
        }

        [Fact]
        public void Should_Pass_Validation_When_Email_Is_Valid()
        {
            // Arrange
            var user = new UserDto("username", "validemail@example.com");

            // Act & Assert
            var result = _validator.TestValidate(user);
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Pass_Validation_When_UserName_Is_Valid()
        {
            // Arrange
            var user = new UserDto("ValidUser", "validemail@example.com");

            // Act & Assert
            var result = _validator.TestValidate(user);
            result.ShouldNotHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public void Should_Pass_Validation_When_All_Fields_Are_Valid()
        {
            // Arrange
            var user = new UserDto("ValidUser", "validemail@example.com");

            // Act & Assert
            var result = _validator.TestValidate(user);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
