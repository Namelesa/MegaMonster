using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MegaMonster.Services.Notification.Core.Interfaces;
using MegaMonster.Services.Notification.Core.User;
using MegaMonster.Services.Notification.WebApi.Notification;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace MegaMonster.Services.Notification.Tests.UnitTests
{
    public class NotificationControllerTests
    {
        private readonly Mock<INotification> _notificationServiceMock;
        private readonly Mock<IValidator<UserDto>> _userValidatorMock;
        private readonly Mock<IValidator<BillUserDto>> _billUserValidatorMock;
        private readonly NotificationController _controller;

        public NotificationControllerTests()
        {
            _notificationServiceMock = new Mock<INotification>();
            _userValidatorMock = new Mock<IValidator<UserDto>>();
            _billUserValidatorMock = new Mock<IValidator<BillUserDto>>();
            _controller = new NotificationController(
                _notificationServiceMock.Object,
                _userValidatorMock.Object,
                _billUserValidatorMock.Object);
        }

        #region SendConfirmEmail Tests

        [Fact]
        public async Task SendConfirmEmail_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var userDto = new UserDto("Test", "test@gmail.com");
            var validationResult = new ValidationResult();
            
            _userValidatorMock
                .Setup(v => v.ValidateAsync(userDto, default))
                .ReturnsAsync(validationResult);
            
            _notificationServiceMock
                .Setup(s => s.SendConfirmEmailAsync(userDto))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SendConfirmEmail(userDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().Be("Send confirm email");
            
            _userValidatorMock.Verify(v => v.ValidateAsync(userDto, default), Times.Once);
            _notificationServiceMock.Verify(s => s.SendConfirmEmailAsync(userDto), Times.Once);
        }

        [Fact]
        public async Task SendConfirmEmail_WithInvalidData_ReturnsBadRequestResult()
        {
            // Arrange
            var userDto = new UserDto("Test", "test@gmail.com");
            var validationError = new ValidationFailure("Email", "Email is required");
            var validationResult = new ValidationResult(new[] { validationError });
            
            _userValidatorMock
                .Setup(v => v.ValidateAsync(userDto, default))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.SendConfirmEmail(userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("Email is required");
            
            _userValidatorMock.Verify(v => v.ValidateAsync(userDto, default), Times.Once);
            _notificationServiceMock.Verify(s => s.SendConfirmEmailAsync(It.IsAny<UserDto>()), Times.Never);
        }

        [Fact]
        public async Task SendConfirmEmail_WhenTemplateNotFound_ReturnsBadRequestResult()
        {
            // Arrange
            var userDto = new UserDto("Test", "test@gmail.com");
            var validationResult = new ValidationResult();
            
            _userValidatorMock
                .Setup(v => v.ValidateAsync(userDto, default))
                .ReturnsAsync(validationResult);
            
            _notificationServiceMock
                .Setup(s => s.SendConfirmEmailAsync(userDto))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.SendConfirmEmail(userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("Template file not found.");
            
            _userValidatorMock.Verify(v => v.ValidateAsync(userDto, default), Times.Once);
            _notificationServiceMock.Verify(s => s.SendConfirmEmailAsync(userDto), Times.Once);
        }

        #endregion

        #region SendBillEmail Tests

        [Fact]
        public async Task SendBillEmail_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var billUserDto = new BillUserDto { /* Заполните необходимые поля */ };
            var validationResult = new ValidationResult();
            
            _billUserValidatorMock
                .Setup(v => v.ValidateAsync(billUserDto, default))
                .ReturnsAsync(validationResult);
            
            _notificationServiceMock
                .Setup(s => s.SendBillEmailAsync(billUserDto))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SendBillEmail(billUserDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().Be("Send Bill email");
            
            _billUserValidatorMock.Verify(v => v.ValidateAsync(billUserDto, default), Times.Once);
            _notificationServiceMock.Verify(s => s.SendBillEmailAsync(billUserDto), Times.Once);
        }

        [Fact]
        public async Task SendBillEmail_WithInvalidData_ReturnsBadRequestResult()
        {
            // Arrange
            var billUserDto = new BillUserDto { /* Заполните необходимые поля */ };
            var validationError = new ValidationFailure("BillId", "BillId is required");
            var validationResult = new ValidationResult(new[] { validationError });
            
            _billUserValidatorMock
                .Setup(v => v.ValidateAsync(billUserDto, default))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.SendBillEmail(billUserDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("BillId is required");
            
            _billUserValidatorMock.Verify(v => v.ValidateAsync(billUserDto, default), Times.Once);
            _notificationServiceMock.Verify(s => s.SendBillEmailAsync(It.IsAny<BillUserDto>()), Times.Never);
        }

        [Fact]
        public async Task SendBillEmail_WhenTemplateNotFound_ReturnsBadRequestResult()
        {
            // Arrange
            var billUserDto = new BillUserDto { /* Заполните необходимые поля */ };
            var validationResult = new ValidationResult();
            
            _billUserValidatorMock
                .Setup(v => v.ValidateAsync(billUserDto, default))
                .ReturnsAsync(validationResult);
            
            _notificationServiceMock
                .Setup(s => s.SendBillEmailAsync(billUserDto))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.SendBillEmail(billUserDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("Template file not found.");
            
            _billUserValidatorMock.Verify(v => v.ValidateAsync(billUserDto, default), Times.Once);
            _notificationServiceMock.Verify(s => s.SendBillEmailAsync(billUserDto), Times.Once);
        }

        #endregion

        #region SendBanEmail Tests

        [Fact]
        public async Task SendBanEmail_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var userDto = new UserDto("Test", "test@gmail.com");
            string reason = "Violation of terms";
            var validationResult = new ValidationResult();
            
            _userValidatorMock
                .Setup(v => v.ValidateAsync(userDto, default))
                .ReturnsAsync(validationResult);
            
            _notificationServiceMock
                .Setup(s => s.SendBanEmailAsync(userDto, reason))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SendBanEmail(userDto, reason);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().Be("Send Ban email");
            
            _userValidatorMock.Verify(v => v.ValidateAsync(userDto, default), Times.Once);
            _notificationServiceMock.Verify(s => s.SendBanEmailAsync(userDto, reason), Times.Once);
        }

        [Fact]
        public async Task SendBanEmail_WithInvalidData_ReturnsBadRequestResult()
        {
            // Arrange
            var userDto = new UserDto("Test", "test@gmail.com");
            string reason = "Violation of terms";
            var validationError = new ValidationFailure("Email", "Email is required");
            var validationResult = new ValidationResult(new[] { validationError });
            
            _userValidatorMock
                .Setup(v => v.ValidateAsync(userDto, default))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.SendBanEmail(userDto, reason);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("Email is required");
            
            _userValidatorMock.Verify(v => v.ValidateAsync(userDto, default), Times.Once);
            _notificationServiceMock.Verify(s => s.SendBanEmailAsync(It.IsAny<UserDto>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendBanEmail_WhenTemplateNotFound_ReturnsBadRequestResult()
        {
            // Arrange
            var userDto = new UserDto("Test", "test@gmail.com");
            string reason = "Violation of terms";
            var validationResult = new ValidationResult();
            
            _userValidatorMock
                .Setup(v => v.ValidateAsync(userDto, default))
                .ReturnsAsync(validationResult);
            
            _notificationServiceMock
                .Setup(s => s.SendBanEmailAsync(userDto, reason))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.SendBanEmail(userDto, reason);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("Template file not found.");
            
            _userValidatorMock.Verify(v => v.ValidateAsync(userDto, default), Times.Once);
            _notificationServiceMock.Verify(s => s.SendBanEmailAsync(userDto, reason), Times.Once);
        }

        #endregion

        #region SendNewsEmail Tests

        [Fact]
        public async Task SendNewsEmail_WithValidData_ReturnsOkResult()
        {
            // Arrange
            var userDto = new UserDto("Test", "test@gmail.com");
            var validationResult = new ValidationResult();
            
            _userValidatorMock
                .Setup(v => v.ValidateAsync(userDto, default))
                .ReturnsAsync(validationResult);
            
            _notificationServiceMock
                .Setup(s => s.SendNewsEmailAsync(userDto))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SendNewsEmail(userDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult?.Value.Should().Be("Send email with news");
            
            _userValidatorMock.Verify(v => v.ValidateAsync(userDto, default), Times.Once);
            _notificationServiceMock.Verify(s => s.SendNewsEmailAsync(userDto), Times.Once);
        }

        [Fact]
        public async Task SendNewsEmail_WithInvalidData_ReturnsBadRequestResult()
        {
            // Arrange
            var userDto = new UserDto("Test", "test@gmail.com");
            var validationError = new ValidationFailure("Email", "Email is required");
            var validationResult = new ValidationResult(new[] { validationError });
            
            _userValidatorMock
                .Setup(v => v.ValidateAsync(userDto, default))
                .ReturnsAsync(validationResult);

            // Act
            var result = await _controller.SendNewsEmail(userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("Email is required");
            
            _userValidatorMock.Verify(v => v.ValidateAsync(userDto, default), Times.Once);
            _notificationServiceMock.Verify(s => s.SendNewsEmailAsync(It.IsAny<UserDto>()), Times.Never);
        }

        [Fact]
        public async Task SendNewsEmail_WhenTemplateNotFound_ReturnsBadRequestResult()
        {
            // Arrange
            var userDto = new UserDto("Test", "test@gmail.com");
            var validationResult = new ValidationResult();
            
            _userValidatorMock
                .Setup(v => v.ValidateAsync(userDto, default))
                .ReturnsAsync(validationResult);
            
            _notificationServiceMock
                .Setup(s => s.SendNewsEmailAsync(userDto))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.SendNewsEmail(userDto);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("Template file not found.");
            
            _userValidatorMock.Verify(v => v.ValidateAsync(userDto, default), Times.Once);
            _notificationServiceMock.Verify(s => s.SendNewsEmailAsync(userDto), Times.Once);
        }

        #endregion
    }
}