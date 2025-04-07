using FluentValidation;
using FluentValidation.Results;
using MegaMonster.Services.User.Application.Role;
using MegaMonster.Services.User.Core.Role;
using Microsoft.Extensions.Logging;
using Moq;

namespace MegaMonster.Services.User.Tests.UnitTests;

public class RoleServiceTests
{
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<ILogger<RoleService>> _loggerMock;
    private readonly Mock<IValidator<Role>> _validatorMock;
    private readonly RoleService _roleService;

    public RoleServiceTests()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _loggerMock = new Mock<ILogger<RoleService>>();
        _validatorMock = new Mock<IValidator<Role>>();
        
        _roleService = new RoleService(
            _roleRepositoryMock.Object,
            _loggerMock.Object,
            _validatorMock.Object);
    }

    #region GetAllRoles

    [Fact]
    public async Task GetAllRoles_ShouldReturnAllRoles()
    {
        // Arrange
        var expectedRoles = new List<Role>
        {
            new("Admin"),
            new("User")
        };
        
        _roleRepositoryMock.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(expectedRoles);

        // Act
        var result = await _roleService.GetAllRoles();

        // Assert
        Assert.Equal(expectedRoles, result);
        _roleRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    #endregion

    #region AddRole

    [Fact]
    public async Task AddRole_WithValidRole_ShouldReturnSuccessResult()
    {
        // Arrange
        var role = new Role("Admin");
        
        _validatorMock.Setup(v => v.ValidateAsync(role, default))
            .ReturnsAsync(new ValidationResult());
        
        _roleRepositoryMock.Setup(repo => repo.AddAsync(role))
            .ReturnsAsync(true);

        // Act
        var result = await _roleService.AddRole(role);

        // Assert
        Assert.True(result.Success);
        _validatorMock.Verify(v => v.ValidateAsync(role, default), Times.Once);
        _roleRepositoryMock.Verify(repo => repo.AddAsync(role), Times.Once);
    }

    [Fact]
    public void Constructor_WithEmptyRoleName_ShouldThrowArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Role(""));
        Assert.Equal("Role name cannot be empty.", ex.Message);
    }

    [Fact]
    public async Task AddRole_WithInvalidRole_ShouldReturnFailResult()
    {
        // Arrange
        var role = new Role("Admin");
        var validationFailures = new List<ValidationFailure>
        {
            new("RoleName", "Role name must be unique")
        };
        
        _validatorMock.Setup(v => v.ValidateAsync(role, default))
            .ReturnsAsync(new ValidationResult(validationFailures));

        // Act
        var result = await _roleService.AddRole(role);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Role name must be unique", result.Message);
        _validatorMock.Verify(v => v.ValidateAsync(role, default), Times.Once);
        _roleRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    public async Task AddRole_WhenRepositoryFails_ShouldReturnFailResult()
    {
        // Arrange
        var role = new Role("Admin");
        
        _validatorMock.Setup(v => v.ValidateAsync(role, default))
            .ReturnsAsync(new ValidationResult());
        
        _roleRepositoryMock.Setup(repo => repo.AddAsync(role))
            .ReturnsAsync(false);

        // Act
        var result = await _roleService.AddRole(role);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Failed to add role.", result.Message);
        _validatorMock.Verify(v => v.ValidateAsync(role, default), Times.Once);
        _roleRepositoryMock.Verify(repo => repo.AddAsync(role), Times.Once);
    }

    [Fact]
    public async Task AddRole_WhenExceptionOccurs_ShouldReturnFailResult()
    {
        // Arrange
        var role = new Role("Admin");
        
        _validatorMock.Setup(v => v.ValidateAsync(role, default))
            .ReturnsAsync(new ValidationResult());
        
        _roleRepositoryMock.Setup(repo => repo.AddAsync(role))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _roleService.AddRole(role);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("An unexpected error occurred.", result.Message);
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((@object, @type) => @object.ToString().Contains("Adding role 'Admin' encountered an error.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region EditRoleAsync

    [Fact]
    public async Task EditRoleAsync_WithValidNames_ShouldReturnSuccessResult()
    {
        // Arrange
        var oldName = "Admin";
        var newName = "SuperAdmin";
        var currentRole = new Role(oldName);
        var roles = new List<Role> { currentRole };
        
        _roleRepositoryMock.Setup(repo => repo.GetRolesByNamesAsync(oldName, newName))
            .ReturnsAsync(roles);
        
        _roleRepositoryMock.Setup(repo => repo.EditAsync(It.Is<Role>(r => r.RoleName == newName)))
            .ReturnsAsync(true);

        // Act
        var result = await _roleService.EditRoleAsync(oldName, newName);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(newName, currentRole.RoleName);
        _roleRepositoryMock.Verify(repo => repo.GetRolesByNamesAsync(oldName, newName), Times.Once);
        _roleRepositoryMock.Verify(repo => repo.EditAsync(currentRole), Times.Once);
    }

    [Fact]
    public async Task EditRoleAsync_WithInvalidNewName_ShouldReturnFailResult()
    {
        // Arrange
        var oldName = "Admin";
        var newName = "ABC";

        // Act
        var result = await _roleService.EditRoleAsync(oldName, newName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("New role name must be at least 4 characters.", result.Message);
        _roleRepositoryMock.Verify(repo => repo.GetRolesByNamesAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task EditRoleAsync_WithNonexistentRole_ShouldReturnFailResult()
    {
        // Arrange
        var oldName = "Admin";
        var newName = "SuperAdmin";
        var roles = new List<Role>();
        
        _roleRepositoryMock.Setup(repo => repo.GetRolesByNamesAsync(oldName, newName))
            .ReturnsAsync(roles);

        // Act
        var result = await _roleService.EditRoleAsync(oldName, newName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal($"Role '{oldName}' not found.", result.Message);
        _roleRepositoryMock.Verify(repo => repo.EditAsync(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    public async Task EditRoleAsync_WithExistingTargetName_ShouldReturnFailResult()
    {
        // Arrange
        var oldName = "Admin";
        var newName = "SuperAdmin";
        var roles = new List<Role>
        {
            new(oldName),
            new(newName)
        };
        
        _roleRepositoryMock.Setup(repo => repo.GetRolesByNamesAsync(oldName, newName))
            .ReturnsAsync(roles);

        // Act
        var result = await _roleService.EditRoleAsync(oldName, newName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal($"Role '{newName}' already exists.", result.Message);
        _roleRepositoryMock.Verify(repo => repo.EditAsync(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    public async Task EditRoleAsync_WhenRepositoryFails_ShouldReturnFailResult()
    {
        // Arrange
        var oldName = "Admin";
        var newName = "SuperAdmin";
        var currentRole = new Role(oldName);
        var roles = new List<Role> { currentRole };
        
        _roleRepositoryMock.Setup(repo => repo.GetRolesByNamesAsync(oldName, newName))
            .ReturnsAsync(roles);
        
        _roleRepositoryMock.Setup(repo => repo.EditAsync(It.Is<Role>(r => r.RoleName == newName)))
            .ReturnsAsync(false);

        // Act
        var result = await _roleService.EditRoleAsync(oldName, newName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Failed to update role.", result.Message);
        _roleRepositoryMock.Verify(repo => repo.EditAsync(It.IsAny<Role>()), Times.Once);
    }

    #endregion

    #region DeleteRoleAsync

    [Fact]
    public async Task DeleteRoleAsync_WithValidName_ShouldReturnSuccessResult()
    {
        // Arrange
        var roleName = "Admin";
        var role = new Role(roleName);
        
        _roleRepositoryMock.Setup(repo => repo.FindRoleByNameAsync(roleName))
            .ReturnsAsync(role);
        
        _roleRepositoryMock.Setup(repo => repo.DeleteAsync(role))
            .ReturnsAsync(true);

        // Act
        var result = await _roleService.DeleteRoleAsync(roleName);

        // Assert
        Assert.True(result.Success);
        _roleRepositoryMock.Verify(repo => repo.FindRoleByNameAsync(roleName), Times.Once);
        _roleRepositoryMock.Verify(repo => repo.DeleteAsync(role), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_WithEmptyName_ShouldReturnFailResult()
    {
        // Arrange
        var roleName = "";

        // Act
        var result = await _roleService.DeleteRoleAsync(roleName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Role name cannot be empty.", result.Message);
        _roleRepositoryMock.Verify(repo => repo.FindRoleByNameAsync(It.IsAny<string>()), Times.Never);
        _roleRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRoleAsync_WithNonexistentRole_ShouldReturnFailResult()
    {
        // Arrange
        var roleName = "Admin";
        
        _roleRepositoryMock.Setup(repo => repo.FindRoleByNameAsync(roleName))
            .ReturnsAsync((Role)null);

        // Act
        var result = await _roleService.DeleteRoleAsync(roleName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal($"Role '{roleName}' not found.", result.Message);
        _roleRepositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<Role>()), Times.Never);
    }

    [Fact]
    public async Task DeleteRoleAsync_WhenRepositoryFails_ShouldReturnFailResult()
    {
        // Arrange
        var roleName = "Admin";
        var role = new Role(roleName);
        
        _roleRepositoryMock.Setup(repo => repo.FindRoleByNameAsync(roleName))
            .ReturnsAsync(role);
        
        _roleRepositoryMock.Setup(repo => repo.DeleteAsync(role))
            .ReturnsAsync(false);

        // Act
        var result = await _roleService.DeleteRoleAsync(roleName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal($"Failed to delete role '{roleName}'.", result.Message);
        _roleRepositoryMock.Verify(repo => repo.DeleteAsync(role), Times.Once);
    }

    #endregion

    #region FindRoleByNameAsync

    [Fact]
    public async Task FindRoleByNameAsync_WithValidName_ShouldReturnRole()
    {
        // Arrange
        var roleName = "Admin";
        var expectedRole = new Role(roleName);
        
        _roleRepositoryMock.Setup(repo => repo.FindRoleByNameAsync(roleName))
            .ReturnsAsync(expectedRole);

        // Act
        var result = await _roleService.FindRoleByNameAsync(roleName);

        // Assert
        Assert.Equal(expectedRole, result);
        _roleRepositoryMock.Verify(repo => repo.FindRoleByNameAsync(roleName), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task FindRoleByNameAsync_WithEmptyName_ShouldReturnNull(string roleName)
    {
        // Act
        var result = await _roleService.FindRoleByNameAsync(roleName);

        // Assert
        Assert.Null(result);
        _roleRepositoryMock.Verify(repo => repo.FindRoleByNameAsync(It.IsAny<string>()), Times.Never);
    }

    #endregion
}