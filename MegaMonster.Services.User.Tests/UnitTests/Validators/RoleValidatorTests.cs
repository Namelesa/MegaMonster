using FluentValidation.TestHelper;
using MegaMonster.Services.User.Application.Role;
using MegaMonster.Services.User.Core.Role;

namespace MegaMonster.Services.User.Tests.UnitTests.Validators;

public class RoleValidationTests
{
    private readonly RoleValidation _validator;

    public RoleValidationTests()
    {
        _validator = new RoleValidation();
    }

    [Fact]
    public void Should_Have_Error_When_RoleName_Is_Empty()
    {
        // Arrange & Act
        var exception = Assert.Throws<ArgumentException>(() => new Role("") { Id = Guid.NewGuid() });

        // Assert
        Assert.Equal("Role name cannot be empty.", exception.Message);
    }

    [Fact]
    public void Should_Have_Error_When_RoleName_Is_Too_Short()
    {
        // Arrange & Act
        var exception = Assert.Throws<ArgumentException>(() => new Role("ABC") { Id = Guid.NewGuid() });

        // Assert
        Assert.Equal("Role name must be at least 4 characters long.", exception.Message);
    }

    [Fact]
    public void Should_Have_Error_When_RoleName_Has_Invalid_Characters()
    {
        // Arrange
        var role = new Role("Admin123") { Id = Guid.NewGuid() };

        // Act & Assert
        var result = _validator.TestValidate(role);
        result.ShouldHaveValidationErrorFor(x => x.RoleName)
            .WithErrorMessage("RoleName can only contain letters.");
    }

    [Fact]
    public void Should_Pass_When_RoleName_Is_Valid()
    {
        // Arrange
        var role = new Role("Admin") { Id = Guid.NewGuid() };

        // Act & Assert
        var result = _validator.TestValidate(role);
        result.ShouldNotHaveValidationErrorFor(x => x.RoleName);
    }
}