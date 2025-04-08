using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using MegaMonster.MessagingModels.User.AddUser;
using MegaMonster.MessagingModels.User.GetInfo;
using MegaMonster.MessagingModels.User.Notification;
using MegaMonster.MessagingModels.User.Tickets;
using MegaMonster.Services.Auth.Application;
using MegaMonster.Services.Auth.Application.User;
using MegaMonster.Services.Auth.Core.User;
using MegaMonster.Services.Auth.Infrastructure.JWT;
using Moq;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace MegaMonster.Services.Auth.Tests.UnitTests.Service
{
    public class AuthServiceTests
    {
        private readonly Mock<IRegisterRepository> _registerRepositoryMock;
        private readonly Mock<ILoginRepository> _loginRepositoryMock;
        private readonly Mock<IJwtService> _jwtServiceMock;
        private readonly Mock<IValidator<Users>> _userValidatorMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly Mock<IRequestClient<UserRequest>> _userRequestClientMock;
        
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _registerRepositoryMock = new Mock<IRegisterRepository>();
            _loginRepositoryMock = new Mock<ILoginRepository>();
            _jwtServiceMock = new Mock<IJwtService>();
            _userValidatorMock = new Mock<IValidator<Users>>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _userRequestClientMock = new Mock<IRequestClient<UserRequest>>();

            _authService = new AuthService(
                _registerRepositoryMock.Object,
                _loginRepositoryMock.Object,
                _jwtServiceMock.Object,
                _userValidatorMock.Object,
                _publishEndpointMock.Object,
                _userRequestClientMock.Object
            );
        }

        #region RegisterUser Tests

        [Fact]
        public async Task RegisterUser_WhenLoginAndEmailAlreadyExist_ShouldReturnFailResult()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "test@example.com";
            const string userName = "TestUser";
            const string login = "testlogin";
            const string phoneNumber = "1234567890";

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterUser(password, email, userName, login, phoneNumber);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Register with the same email");
        }

        [Fact]
        public async Task RegisterUser_WhenUserIsBanned_ShouldReturnFailResult()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "test@example.com";
            const string userName = "TestUser";
            const string login = "testlogin";
            const string phoneNumber = "1234567890";

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(false);

            _loginRepositoryMock
                .Setup(x => x.FindUser(login))
                .ReturnsAsync(new Users { IsBan = true });

            // Act
            var result = await _authService.RegisterUser(password, email, userName, login, phoneNumber);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("user is baned");
        }

        [Fact]
        public async Task RegisterUser_WhenValidationFails_ShouldReturnFailResult()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "test@example.com";
            const string userName = "TestUser";
            const string login = "testlogin";
            const string phoneNumber = "1234567890";

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(false);

            _loginRepositoryMock
                .Setup(x => x.FindUser(login))
                .ReturnsAsync((Users)null);

            _registerRepositoryMock
                .Setup(x => x.HashPassword(password, It.IsAny<Users>()))
                .ReturnsAsync("hashedPassword");

            var validationFailure = new ValidationFailure("Email", "Invalid email format");
            var validationResult = new ValidationResult(new[] { validationFailure });

            _userValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<Users>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _authService.RegisterUser(password, email, userName, login, phoneNumber);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task RegisterUser_WhenCustomerRegistrationSucceeds_ShouldReturnSuccessResult()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "test@example.com";
            const string userName = "TestUser";
            const string login = "testlogin";
            const string phoneNumber = "1234567890";

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(false);

            _loginRepositoryMock
                .Setup(x => x.FindUser(login))
                .ReturnsAsync((Users)null);

            _registerRepositoryMock
                .Setup(x => x.HashPassword(password, It.IsAny<Users>()))
                .ReturnsAsync("hashedPassword");

            _userValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<Users>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _registerRepositoryMock
                .Setup(x => x.RegisterUser(It.IsAny<Users>()))
                .ReturnsAsync(true);

            _publishEndpointMock
                .Setup(x => x.Publish(It.IsAny<UserModelMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.RegisterUser(password, email, userName, login, phoneNumber);

            // Assert
            result.Success.Should().BeTrue();

            // Verify publish was called with correct user details
            _publishEndpointMock.Verify(
                x => x.Publish(It.Is<UserModelMessage>(
                    msg => msg.Login == login && 
                           msg.UserName == userName && 
                           msg.Email == email &&
                           msg.PhoneNumber == phoneNumber), 
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RegisterUser_WhenAdminRegistrationSucceeds_ShouldReturnSuccessResult()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "admin@example.com";
            const string userName = "AdminUser";
            const string login = "adminlogin";
            const string phoneNumber = "1234567890";
            const string role = Wc.AdminRole;

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(false);

            _loginRepositoryMock
                .Setup(x => x.FindUser(login))
                .ReturnsAsync((Users)null);

            _registerRepositoryMock
                .Setup(x => x.HashPassword(password, It.IsAny<Users>()))
                .ReturnsAsync("hashedPassword");

            _userValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<Users>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _registerRepositoryMock
                .Setup(x => x.RegisterUser(It.IsAny<Users>()))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.RegisterUser(password, email, userName, login, phoneNumber, role);

            // Assert
            result.Success.Should().BeTrue();

            // Verify that publish was not called for admin registration
            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<UserModelMessage>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RegisterUser_WhenRegistrationFails_ShouldReturnFailResult()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "test@example.com";
            const string userName = "TestUser";
            const string login = "testlogin";
            const string phoneNumber = "1234567890";

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(false);

            _loginRepositoryMock
                .Setup(x => x.FindUser(login))
                .ReturnsAsync((Users)null);

            _registerRepositoryMock
                .Setup(x => x.HashPassword(password, It.IsAny<Users>()))
                .ReturnsAsync("hashedPassword");

            _userValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<Users>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _registerRepositoryMock
                .Setup(x => x.RegisterUser(It.IsAny<Users>()))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.RegisterUser(password, email, userName, login, phoneNumber);

            // Asser
            result.Success.Should().BeFalse();
        }

        #endregion

        #region LoginUser Tests

        [Fact]
        public async Task LoginUser_WhenLoginAndEmailNotFound_ShouldReturnFailResult()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "test@example.com";
            const string login = "testlogin";

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.LoginUser(password, email, login);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Register with login");
        }

        [Fact]
        public async Task LoginUser_WhenUserNotFound_ShouldReturnFailResult()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "test@example.com";
            const string login = "testlogin";

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(true);

            _loginRepositoryMock
                .Setup(x => x.FindUser(login))
                .ReturnsAsync((Users)null);

            // Act
            var result = await _authService.LoginUser(password, email, login);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Register not found");
        }

        [Fact]
        public async Task LoginUser_WhenEmailNotConfirmed_ShouldReturnFailResult()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "test@example.com";
            const string login = "testlogin";

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(true);

            _loginRepositoryMock
                .Setup(x => x.FindUser(login))
                .ReturnsAsync(new Users { EmailConfirmed = false });

            // Act
            var result = await _authService.LoginUser(password, email, login);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Please confirm email");
        }

        [Fact]
        public async Task LoginUser_WhenValidationFails_ShouldReturnFailResult()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "test@example.com";
            const string login = "testlogin";

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(true);

            _loginRepositoryMock
                .Setup(x => x.FindUser(login))
                .ReturnsAsync(new Users { EmailConfirmed = true });

            var validationFailure = new ValidationFailure("Email", "Invalid email format");
            var validationResult = new ValidationResult(new[] { validationFailure });

            _userValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<Users>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _authService.LoginUser(password, email, login);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Invalid email format");
        }

        [Fact]
        public async Task LoginUser_WhenUserNotFoundInUserService_ShouldReturnFailResult()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "test@example.com";
            const string login = "testlogin";

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(true);

            var user = new Users { EmailConfirmed = true };
            _loginRepositoryMock
                .Setup(x => x.FindUser(login))
                .ReturnsAsync(user);

            _userValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<Users>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _registerRepositoryMock
                .Setup(x => x.HashPassword(password, It.IsAny<Users>()))
                .ReturnsAsync("hashedPassword");

            // Setup the response from UserService with empty UserId
            var mockResponse = new Mock<Response<UserTicketModel>>();
            mockResponse.Setup(r => r.Message).Returns(new UserTicketModel(string.Empty, user.UserName));

            _userRequestClientMock
                .Setup(x => x.GetResponse<UserTicketModel>(It.IsAny<UserRequest>(), It.IsAny<CancellationToken>(), It.IsAny<RequestTimeout>()))
                .ReturnsAsync(mockResponse.Object);

            // Act
            var result = await _authService.LoginUser(password, email, login);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Register not found in UserService");
        }

        [Fact]
        public async Task LoginUser_WhenLoginSucceeds_ShouldReturnSuccessResult()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "test@example.com";
            const string login = "testlogin";
            const string userId = "user123";
            const string role = "Customer";
            const string jwtToken = "jwt.token.here";

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(true);

            var user = new Users { EmailConfirmed = true, Role = role };
            _loginRepositoryMock
                .Setup(x => x.FindUser(login))
                .ReturnsAsync(user);

            _userValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<Users>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _registerRepositoryMock
                .Setup(x => x.HashPassword(password, It.IsAny<Users>()))
                .ReturnsAsync("hashedPassword");

            // Setup the response from UserService
            var mockResponse = new Mock<Response<UserTicketModel>>();
            mockResponse.Setup(r => r.Message).Returns(new UserTicketModel(userId, user.UserName));

            _userRequestClientMock
                .Setup(x => x.GetResponse<UserTicketModel>(It.IsAny<UserRequest>(), It.IsAny<CancellationToken>(), It.IsAny<RequestTimeout>()))
                .ReturnsAsync(mockResponse.Object);

            _jwtServiceMock
                .Setup(x => x.AuthenticateAsync(It.IsAny<Users>(), password, role))
                .ReturnsAsync(jwtToken);

            // Act
            var result = await _authService.LoginUser(password, email, login);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be(jwtToken);

            // Verify user's ID was set from the response
            user.Id.Should().Be(userId);
        }

        [Fact]
        public async Task LoginUser_WhenExceptionThrown_ShouldRethrowException()
        {
            // Arrange
            const string password = "StrongPassword123";
            const string email = "test@example.com";
            const string login = "testlogin";

            _registerRepositoryMock
                .Setup(x => x.CheckLoginAndEmail(login, email))
                .ReturnsAsync(true);

            var user = new Users { EmailConfirmed = true };
            _loginRepositoryMock
                .Setup(x => x.FindUser(login))
                .ReturnsAsync(user);

            _userValidatorMock
                .Setup(x => x.ValidateAsync(It.IsAny<Users>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _registerRepositoryMock
                .Setup(x => x.HashPassword(password, It.IsAny<Users>()))
                .ReturnsAsync("hashedPassword");

            // Setup exception
            _userRequestClientMock
                .Setup(x => x.GetResponse<UserTicketModel>(It.IsAny<UserRequest>(), It.IsAny<CancellationToken>(), It.IsAny<RequestTimeout>()))
                .ThrowsAsync(new Exception("Connection error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _authService.LoginUser(password, email, login));
        }

        #endregion

        #region BanUser Tests

        [Fact]
        public async Task BanUser_WhenUserNotFound_ShouldReturnFailResult()
        {
            // Arrange
            const string email = "test@example.com";
            
            _registerRepositoryMock
                .Setup(x => x.BanUser(email))
                .ReturnsAsync("Register not found");

            // Act
            var result = await _authService.BanUser(email);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Register not found");
        }

        [Fact]
        public async Task BanUser_WhenUserBanned_ShouldReturnSuccessResult()
        {
            // Arrange
            const string email = "test@example.com";
            const string successMessage = "User banned successfully";
            
            _registerRepositoryMock
                .Setup(x => x.BanUser(email))
                .ReturnsAsync(successMessage);

            // Act
            var result = await _authService.BanUser(email);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be(successMessage);
        }

        #endregion

        #region DeleteUser Tests

        [Fact]
        public async Task DeleteUser_WhenDeleteSucceeds_ShouldReturnSuccessResult()
        {
            // Arrange
            const string login = "testlogin";
            
            _registerRepositoryMock
                .Setup(x => x.DeleteUserByLogin(login))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.DeleteUser(login);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Contain("was deleted");
        }

        [Fact]
        public async Task DeleteUser_WhenDeleteFails_ShouldReturnFailResult()
        {
            // Arrange
            const string login = "testlogin";
            
            _registerRepositoryMock
                .Setup(x => x.DeleteUserByLogin(login))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.DeleteUser(login);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Contain("Can not delete user");
        }

        #endregion

        #region ConfirmEmail Tests

        [Fact]
        public async Task ConfirmEmail_WhenUserNotFound_ShouldReturnFailResult()
        {
            // Arrange
            const string email = "test@example.com";
            
            _registerRepositoryMock
                .Setup(x => x.FindUserByEmail(email))
                .ReturnsAsync((Users)null);

            // Act
            var result = await _authService.ConfirmEmail(email);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Register not found");
        }

        [Fact]
        public async Task ConfirmEmail_WhenConfirmationSucceeds_ShouldReturnSuccessResult()
        {
            // Arrange
            const string email = "test@example.com";
            const string login = "testlogin";
            
            var user = new Users { Email = email, Login = login };
            
            _registerRepositoryMock
                .Setup(x => x.FindUserByEmail(email))
                .ReturnsAsync(user);
                
            _registerRepositoryMock
                .Setup(x => x.ConfirmEmailAsync(user))
                .ReturnsAsync(true);
                
            _publishEndpointMock
                .Setup(x => x.Publish(It.IsAny<ConfirmEmailUser>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _authService.ConfirmEmail(email);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Email confirmed successfully");
            
            // Verify that publish was called
            _publishEndpointMock.Verify(
                x => x.Publish(It.Is<ConfirmEmailUser>(msg => msg.Login == login), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ConfirmEmail_WhenConfirmationFails_ShouldReturnFailResult()
        {
            // Arrange
            const string email = "test@example.com";
            
            var user = new Users { Email = email };
            
            _registerRepositoryMock
                .Setup(x => x.FindUserByEmail(email))
                .ReturnsAsync(user);
                
            _registerRepositoryMock
                .Setup(x => x.ConfirmEmailAsync(user))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.ConfirmEmail(email);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Invalid or expired token");
            
            // Verify that publish was NOT called
            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<ConfirmEmailUser>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        #endregion

        #region EditUser Tests

        [Fact]
        public async Task EditUser_WhenUserNotFound_ShouldReturnFailResult()
        {
            // Arrange
            const string oldLogin = "oldlogin";
            const string userName = "UpdatedName";
            const string email = "updated@example.com";
            const string phoneNumber = "9876543210";
            const string newLogin = "newlogin";
            
            _loginRepositoryMock
                .Setup(x => x.FindUser(oldLogin))
                .ReturnsAsync((Users)null);

            // Act
            var result = await _authService.EditUser(oldLogin, userName, email, phoneNumber, newLogin);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Register not found");
        }

        [Fact]
        public async Task EditUser_WhenUpdateSucceeds_ShouldReturnSuccessResult()
        {
            // Arrange
            const string oldLogin = "oldlogin";
            const string userName = "UpdatedName";
            const string email = "updated@example.com";
            const string phoneNumber = "9876543210";
            const string newLogin = "newlogin";
            
            var user = new Users { Login = oldLogin };
            
            _loginRepositoryMock
                .Setup(x => x.FindUser(oldLogin))
                .ReturnsAsync(user);
                
            _loginRepositoryMock
                .Setup(x => x.UpdateUser(It.IsAny<Users>()))
                .ReturnsAsync(true);

            // Act
            var result = await _authService.EditUser(oldLogin, userName, email, phoneNumber, newLogin);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Register edited successfully");
            
            // Verify user properties were updated
            user.UserName.Should().Be(userName);
            user.Login.Should().Be(newLogin);
            user.PhoneNumber.Should().Be(phoneNumber);
            user.Email.Should().Be(email);
            user.NormalizedUserName.Should().Be(userName.ToUpper());
            user.NormalizedEmail.Should().Be(email.ToUpper());
        }

        [Fact]
        public async Task EditUser_WhenUpdateFails_ShouldReturnFailResult()
        {
            // Arrange
            const string oldLogin = "oldlogin";
            const string userName = "UpdatedName";
            const string email = "updated@example.com";
            const string phoneNumber = "9876543210";
            const string newLogin = "newlogin";
            
            var user = new Users { Login = oldLogin };
            
            _loginRepositoryMock
                .Setup(x => x.FindUser(oldLogin))
                .ReturnsAsync(user);
                
            _loginRepositoryMock
                .Setup(x => x.UpdateUser(It.IsAny<Users>()))
                .ReturnsAsync(false);

            // Act
            var result = await _authService.EditUser(oldLogin, userName, email, phoneNumber, newLogin);

            // Assert
            result.Should().NotBeNull();
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error with editing user info");
        }

        #endregion

        #region FindUser Tests

        [Fact]
        public async Task FindUserByEmail_ShouldCallRepository()
        {
            // Arrange
            const string email = "test@example.com";
            var expectedUser = new Users { Email = email };
            
            _loginRepositoryMock
                .Setup(x => x.FindByEmail(email))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _authService.FindUserByEmail(email);

            // Assert
            result.Should().BeSameAs(expectedUser);
            _loginRepositoryMock.Verify(x => x.FindByEmail(email), Times.Once);
        }

        [Fact]
        public async Task FindUserByLogin_ShouldCallRepository()
        {
            // Arrange
            const string login = "testlogin";
            var expectedUser = new Users { Login = login };
            
            _loginRepositoryMock
                .Setup(x => x.FindUser(login))
                .ReturnsAsync(expectedUser);

            // Act
            var result = await _authService.FindUserByLogin(login);

            // Assert
            result.Should().BeSameAs(expectedUser);
            _loginRepositoryMock.Verify(x => x.FindUser(login), Times.Once);
        }

        #endregion
    }
}