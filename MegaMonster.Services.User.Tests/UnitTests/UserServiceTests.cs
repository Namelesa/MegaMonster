using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using MegaMonster.MessagingModels.User.AddAdmin;
using MegaMonster.MessagingModels.User.Edit;
using MegaMonster.MessagingModels.User.Notification;
using MegaMonster.MessagingModels.User.Notification.Ban;
using MegaMonster.Services.User.Application;
using MegaMonster.Services.User.Application.User;
using MegaMonster.Services.User.Core.Role;
using MegaMonster.Services.User.Core.User;
using MegaMonster.Services.User.Infrastructure.Redis;
using Moq;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace MegaMonster.Services.User.Tests.UnitTests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IValidator<Users>> _validationMock;
    private readonly Mock<IRedisService> _redisServiceMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IBannedUserRepository> _bannedUserRepositoryMock;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _validationMock = new Mock<IValidator<Users>>();
        _redisServiceMock = new Mock<IRedisService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _bannedUserRepositoryMock = new Mock<IBannedUserRepository>();

        _userService = new UserService(
            _userRepositoryMock.Object,
            _validationMock.Object,
            _redisServiceMock.Object,
            _publishEndpointMock.Object,
            _bannedUserRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllUsers_ShouldReturnCachedData_WhenCacheExists()
    {
        // Arrange
        var expectedUsers = new List<Users> { CreateTestUser() };
        _redisServiceMock.Setup(x => x.GetAsync<IEnumerable<Users>>("All_Users"))
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.GetAllUsers();

        // Assert
        result.Should().BeEquivalentTo(expectedUsers);
        _userRepositoryMock.Verify(x => x.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetAllUsers_ShouldFetchFromRepository_WhenCacheDoesNotExist()
    {
        // Arrange
        var expectedUsers = new List<Users> { CreateTestUser() };
        _redisServiceMock.Setup(x => x.GetAsync<IEnumerable<Users>>("All_Users"))
            .ReturnsAsync((IEnumerable<Users>)null);
        _userRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(expectedUsers);

        // Act
        var result = await _userService.GetAllUsers();

        // Assert
        result.Should().BeEquivalentTo(expectedUsers);
        _userRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
        _redisServiceMock.Verify(x => x.SetAsync(
            "All_Users", 
            It.IsAny<IEnumerable<Users>>(), 
            It.IsAny<TimeSpan>()), 
            Times.Once);
    }

    [Fact]
    public async Task FindByLoginAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var expectedUser = CreateTestUser();
        var login = "testuser";
        _userRepositoryMock.Setup(x => x.GetUserByLoginAsync(login))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.FindByLoginAsync(login);

        // Assert
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task FindByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var expectedUser = CreateTestUser();
        var userId = Guid.NewGuid();
        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.FindByIdAsync(userId);

        // Assert
        result.Should().BeEquivalentTo(expectedUser);
    }

    [Fact]
    public async Task AddUser_ShouldReturnSuccess_WhenValidationPasses_AndRoleIsCustomer()
    {
        // Arrange
        var user = CreateTestUser();
        SetupSuccessfulValidation(user);
        _userRepositoryMock.Setup(x => x.AddAsync(user))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.AddUser(user, Wc.CustomerRole);

        // Assert
        result.Success.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.AddAsync(user), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<UserNotificationBase>(n => 
                n.Email == user.Email && 
                n.UserName == user.UserName),
            It.IsAny<CancellationToken>()), 
            Times.Once);
        _redisServiceMock.Verify(x => x.RemoveAsync("All_Users"), Times.Once);
    }

    [Fact]
    public async Task AddUser_ShouldReturnSuccess_WhenValidationPasses_AndRoleIsAdmin()
    {
        // Arrange
        var user = CreateTestUser();
        string role = "Admin";
        SetupSuccessfulValidation(user);
        _userRepositoryMock.Setup(x => x.AddAsync(user))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.AddUser(user, role);

        // Assert
        result.Success.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.AddAsync(user), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<AddAdminModel>(a => 
                a.Login == user.Login && 
                a.Email == user.Email &&
                a.Role == role),
            It.IsAny<CancellationToken>()), 
            Times.Once);
        _redisServiceMock.Verify(x => x.RemoveAsync("All_Users"), Times.Once);
    }

    [Fact]
    public async Task AddUser_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var user = CreateTestUser();
        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Email", "Invalid email format")
        };
        SetupFailedValidation(user, validationErrors);

        // Act
        var result = await _userService.AddUser(user, Wc.CustomerRole);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid email format");
        _userRepositoryMock.Verify(x => x.AddAsync(user), Times.Never);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddUser_ShouldReturnFailure_WhenRepositoryOperationFails()
    {
        // Arrange
        var user = CreateTestUser();
        SetupSuccessfulValidation(user);
        _userRepositoryMock.Setup(x => x.AddAsync(user))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.AddUser(user, Wc.CustomerRole);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Failed to add user");
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        _redisServiceMock.Verify(x => x.RemoveAsync("All_Users"), Times.Never);
    }

    [Fact]
    public async Task EditUser_ShouldReturnSuccess_WhenValidationPasses()
    {
        // Arrange
        var user = CreateTestUser();
        string login = "oldlogin";
        string newLogin = "newlogin";
        string userName = "New User Name";
        string email = "new@example.com";
        string phoneNumber = "1234567890";

        _userRepositoryMock.Setup(x => x.GetUserByLoginAsync(login))
            .ReturnsAsync(user);

        SetupSuccessfulValidation(user);

        _userRepositoryMock.Setup(x => x.EditAsync(It.IsAny<Users>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.EditUser(login, userName, email, phoneNumber, newLogin);

        // Assert
        result.Success.Should().BeTrue();

        _userRepositoryMock.Verify(x => x.EditAsync(It.Is<Users>(u =>
                u.Login == newLogin &&
                u.UserName == userName &&
                u.Email == email &&
                u.PhoneNumber == phoneNumber)),
            Times.Once);

        _publishEndpointMock.Verify(x => x.Publish(
                It.Is<UserEditMessage>(m =>
                    m.NewLogin == newLogin &&
                    m.OldLogin == login),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _redisServiceMock.Verify(x => x.RemoveAsync("All_Users"), Times.Once);
    }

    [Fact]
    public async Task EditUser_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        string login = "nonexistent";
        _userRepositoryMock.Setup(x => x.GetUserByLoginAsync(login))
            .ReturnsAsync((Users)null);

        // Act
        var result = await _userService.EditUser(login, "name", "email", "phone", "newlogin");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain($"Register with login '{login}' not found");
        _userRepositoryMock.Verify(x => x.EditAsync(It.IsAny<Users>()), Times.Never);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EditUser_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var user = CreateTestUser();
        string login = "oldlogin";
        string newLogin = "newlogin";
        string userName = "New User Name";
        string email = "new@example.com";
        string phoneNumber = "1234567890";

        _userRepositoryMock.Setup(x => x.GetUserByLoginAsync(login))
            .ReturnsAsync(user);
        
        SetupSuccessfulValidation();

        _userRepositoryMock.Setup(x => x.EditAsync(It.IsAny<Users>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.EditUser(login, userName, email, phoneNumber, newLogin);

        // Assert
        result.Success.Should().BeTrue();

        _userRepositoryMock.Verify(x => x.EditAsync(It.Is<Users>(u =>
                u.Login == newLogin &&
                u.UserName == userName &&
                u.Email == email &&
                u.PhoneNumber == phoneNumber)),
            Times.Once);

        _publishEndpointMock.Verify(x => x.Publish(
                It.Is<UserEditMessage>(m =>
                    m.NewLogin == newLogin &&
                    m.OldLogin == login),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _redisServiceMock.Verify(x => x.RemoveAsync("All_Users"), Times.Once);
    }

    [Fact]
    public async Task EditUserRollBack_ShouldReturnSuccess_WhenValidationPasses()
    {
        // Arrange
        var user = CreateTestUser();
        string login = "oldlogin";
        string newLogin = "newlogin";
        string userName = "New User Name";
        string email = "new@example.com";
        string phoneNumber = "1234567890";

        _userRepositoryMock.Setup(x => x.GetUserByLoginAsync(login))
            .ReturnsAsync(user);

        SetupSuccessfulValidation();

        _userRepositoryMock.Setup(x => x.EditAsync(It.IsAny<Users>()))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.EditUser(login, userName, email, phoneNumber, newLogin);

        // Assert
        result.Success.Should().BeTrue();

        _userRepositoryMock.Verify(x => x.EditAsync(It.Is<Users>(u =>
                u.Login == newLogin &&
                u.UserName == userName &&
                u.Email == email &&
                u.PhoneNumber == phoneNumber)),
            Times.Once);

        _publishEndpointMock.Verify(x => x.Publish(
                It.Is<UserEditMessage>(m =>
                    m.NewLogin == newLogin &&
                    m.OldLogin == login),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _redisServiceMock.Verify(x => x.RemoveAsync("All_Users"), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var user = CreateTestUser();
        string login = "testuser";
        string reason = "Violation of terms";

        _userRepositoryMock.Setup(x => x.GetUserByLoginAsync(login))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.DeleteAsync(user))
            .ReturnsAsync(true);
        _bannedUserRepositoryMock.Setup(x => x.AddAsync(user))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.DeleteUser(login, reason);

        // Assert
        result.Success.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.DeleteAsync(user), Times.Once);
        _bannedUserRepositoryMock.Verify(x => x.AddAsync(user), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<UserBan>(b => 
                b.Email == user.Email && 
                b.Reason == reason),
            It.IsAny<CancellationToken>()), 
            Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<UserBanForAuth>(b => b.Email == user.Email),
            It.IsAny<CancellationToken>()), 
            Times.Once);
        _redisServiceMock.Verify(x => x.RemoveAsync("All_Users"), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        string login = "nonexistent";
        _userRepositoryMock.Setup(x => x.GetUserByLoginAsync(login))
            .ReturnsAsync((Users)null);

        // Act
        var result = await _userService.DeleteUser(login, "reason");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain($"Register with login '{login}' not found");
        _userRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Users>()), Times.Never);
        _bannedUserRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Users>()), Times.Never);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var user = CreateTestUser();
        string login = "testuser";

        _userRepositoryMock.Setup(x => x.GetUserByLoginAsync(login))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.ConfirmEmailAsync(user))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.ConfirmEmail(login);

        // Assert
        result.Success.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.ConfirmEmailAsync(user), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmail_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        string login = "nonexistent";
        _userRepositoryMock.Setup(x => x.GetUserByLoginAsync(login))
            .ReturnsAsync((Users)null);

        // Act
        var result = await _userService.ConfirmEmail(login);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Register not found");
        _userRepositoryMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<Users>()), Times.Never);
    }

    [Fact]
    public async Task UserBanRestoreAsync_ShouldReturnSuccess_WhenUserExists()
    {
        // Arrange
        var user = CreateTestUser();
        string login = "testuser";

        _bannedUserRepositoryMock.Setup(x => x.FindByLoginAsync(login))
            .ReturnsAsync(user);
        _userRepositoryMock.Setup(x => x.AddAsync(user))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.UserBanRestoreAsync(login);

        // Assert
        result.Success.Should().BeTrue();
        _userRepositoryMock.Verify(x => x.AddAsync(user), Times.Once);
    }

    [Fact]
    public async Task UserBanRestoreAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Arrange
        string login = "nonexistent";
        _bannedUserRepositoryMock.Setup(x => x.FindByLoginAsync(login))
            .ReturnsAsync((Users)null);

        // Act
        var result = await _userService.UserBanRestoreAsync(login);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Register not found");
        _userRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Users>()), Times.Never);
    }

    #region Helper Methods
    
    private Users CreateTestUser()
    {
        return new Users("TestLogin1234")
        {
            UserName = "Test User",
            NormalizedUserName = "TEST USER",
            Email = "test@example.com",
            NormalizedEmail = "TEST@EXAMPLE.COM",
            PhoneNumber = "1234567890",
            PasswordHash = "someHash",
            Role = new Role("User") { Id = Guid.NewGuid()} // ВАЖНО!
        };
    }


    private void SetupSuccessfulValidation(Users user)
    {
        _validationMock.Setup(x => x.ValidateAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private void SetupFailedValidation(Users user, List<ValidationFailure> errors)
    {
        _validationMock.Setup(x => x.ValidateAsync(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(errors));
    }
    
    private void SetupSuccessfulValidation()
    {
        var validationResult = new ValidationResult();

        _validationMock.Setup(v => v.ValidateAsync(It.IsAny<Users>(), default))
            .ReturnsAsync(validationResult);
    }
    #endregion
}
