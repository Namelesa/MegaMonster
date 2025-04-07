using MassTransit;
using MegaMonster.MessagingModels.Card;
using MegaMonster.Services.Favors.Application.Ticket;
using MegaMonster.Services.Favors.Core.Ticket;
using MegaMonster.Services.Favors.Core.TicketConfiguration;
using MegaMonster.Services.Favors.Infrastructure.Redis;
using Moq;

namespace MegaMonster.Services.Favors.Tests.UnitTests
{
    public class TicketsServiceTests
    {
        private readonly Mock<ITicketRepository> _mockTicketRepo;
        private readonly Mock<ITicketConfigurationRepository> _mockConfigRepo;
        private readonly Mock<IRedisService> _mockRedisService;
        private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;
        private readonly TicketsService _service;

        public TicketsServiceTests()
        {
            _mockTicketRepo = new Mock<ITicketRepository>();
            _mockConfigRepo = new Mock<ITicketConfigurationRepository>();
            _mockRedisService = new Mock<IRedisService>();
            _mockPublishEndpoint = new Mock<IPublishEndpoint>();
            
            _service = new TicketsService(
                _mockTicketRepo.Object,
                _mockConfigRepo.Object,
                _mockRedisService.Object,
                _mockPublishEndpoint.Object
            );
        }

        #region GetAllTicketsByStatus Tests

        [Fact]
        public async Task GetAllTicketsByStatus_WhenCacheExists_ShouldReturnCachedData()
        {
            // Arrange
            var status = "Active";
            var cacheKey = $"Tickets_Status_{status}";
            var cachedTickets = new List<Ticket>
            {
                new() { Id = 1, UserType = status },  
                new() { Id = 2, UserType = status }  
            };

            _mockRedisService
                .Setup(x => x.GetAsync<List<Ticket>>(cacheKey))
                .ReturnsAsync(cachedTickets);

            // Act
            var result = await _service.GetAllTicketsByStatus(status);

            // Assert
            Assert.Equal(cachedTickets, result);
            _mockRedisService.Verify(x => x.GetAsync<List<Ticket>>(cacheKey), Times.Once); 
            _mockTicketRepo.Verify(x => x.GetTicketByStatus(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAllTicketsByStatus_WhenCacheNotExists_ShouldFetchFromRepositoryAndCache()
        {
            // Arrange
            var status = "Active";
            var cacheKey = $"Tickets_Status_{status}";
            var tickets = new List<Ticket>
            {
                new() { Id = 1 },
                new() { Id = 2 }
            };

            _mockRedisService
                .Setup(x => x.GetAsync<List<Ticket>>(cacheKey))
                .ReturnsAsync((List<Ticket>)null);

            _mockTicketRepo
                .Setup(x => x.GetTicketByStatus(status))
                .ReturnsAsync(tickets);

            // Act
            var result = await _service.GetAllTicketsByStatus(status);

            // Assert
            Assert.Equal(tickets, result);
            _mockRedisService.Verify(x => x.GetAsync<List<Ticket>>(cacheKey), Times.Once);
            _mockTicketRepo.Verify(x => x.GetTicketByStatus(status), Times.Once);
            _mockRedisService.Verify(x => x.SetAsync(
                    cacheKey,
                    tickets,
                    It.Is<TimeSpan>(t => t == TimeSpan.FromMinutes(60))),
                Times.Once);
        }

        #endregion

        #region GetTicketById Tests

        [Fact]
        public async Task GetTicketById_ShouldReturnTicketFromRepository()
        {
            // Arrange
            var ticketId = 1;
            var ticket = new Ticket { Id = ticketId };

            _mockTicketRepo
                .Setup(x => x.GetTicketById(ticketId))
                .ReturnsAsync(ticket);

            // Act
            var result = await _service.GetTicketById(ticketId);

            // Assert
            Assert.Equal(ticket, result);
            _mockTicketRepo.Verify(x => x.GetTicketById(ticketId), Times.Once);
        }

        #endregion

        #region GetAllTicketConfigurations Tests

        [Fact]
        public async Task GetAllTicketConfigurations_WhenCacheExists_ShouldReturnCachedData()
        {
            // Arrange
            const string cacheKey = "All_Ticket_Configurations";
            var cachedConfigs = new List<TicketConfiguration>
            {
                new() { UserType = "Student", Price = 50 },
                new() { UserType = "Adult", Price = 100 }
            };

            _mockRedisService
                .Setup(x => x.GetAsync<TicketConfiguration[]>(cacheKey))
                .ReturnsAsync(cachedConfigs.ToArray());

            // Act
            var result = await _service.GetAllTicketConfigurations();

            // Assert
            Assert.Equal(cachedConfigs, result);
            _mockRedisService.Verify(x => x.GetAsync<IEnumerable<TicketConfiguration>>(cacheKey), Times.Once);
            _mockConfigRepo.Verify(x => x.GetAll(), Times.Never);
        }

        [Fact]
        public async Task GetAllTicketConfigurations_WhenCacheNotExists_ShouldFetchFromRepositoryAndCache()
        {
            // Arrange
            const string cacheKey = "All_Ticket_Configurations";
            var configs = new List<TicketConfiguration>
            {
                new() { UserType = "Student", Price = 50 },
                new() { UserType = "Adult", Price = 100 }
            };

            _mockRedisService
                .Setup(x => x.GetAsync<TicketConfiguration[]>(cacheKey))
                .ReturnsAsync((TicketConfiguration[])null);

            _mockConfigRepo
                .Setup(x => x.GetAll())
                .ReturnsAsync(configs);

            // Act
            var result = await _service.GetAllTicketConfigurations();

            // Assert
            Assert.Equal(configs.ToArray(), result);
            _mockRedisService.Verify(x => x.GetAsync<TicketConfiguration[]>(cacheKey), Times.Once);
            _mockConfigRepo.Verify(x => x.GetAll(), Times.Once);
            _mockRedisService.Verify(x => x.SetAsync(
                    cacheKey,
                    It.Is<TicketConfiguration[]>(c => c.Length == configs.Count),
                    It.Is<TimeSpan>(t => t == TimeSpan.FromMinutes(60))),
                Times.Once);
        }

        #endregion

        #region GetConfigurationForUser Tests

        [Fact]
        public async Task GetConfigurationForUser_WhenCacheExists_ShouldReturnCachedData()
        {
            // Arrange
            var userType = "Student";
            var cacheKey = $"TicketConfig_{userType}";
            var cachedConfig = new TicketConfiguration { UserType = userType, Price = 50 };

            _mockRedisService
                .Setup(x => x.GetAsync<TicketConfiguration>(cacheKey))
                .ReturnsAsync(cachedConfig);

            // Act
            var result = await _service.GetConfigurationForUser(userType);

            // Assert
            Assert.Equal(cachedConfig, result);
            _mockRedisService.Verify(x => x.GetAsync<TicketConfiguration>(cacheKey), Times.Once);
            _mockConfigRepo.Verify(x => x.GetConfigurationAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetConfigurationForUser_WhenCacheNotExists_ShouldFetchFromRepositoryAndCache()
        {
            // Arrange
            var userType = "Student";
            var cacheKey = $"TicketConfig_{userType}";
            var config = new TicketConfiguration { UserType = userType, Price = 50 };

            _mockRedisService
                .Setup(x => x.GetAsync<TicketConfiguration>(cacheKey))
                .ReturnsAsync((TicketConfiguration)null);

            _mockConfigRepo
                .Setup(x => x.GetConfigurationAsync(userType))
                .ReturnsAsync(config);

            // Act
            var result = await _service.GetConfigurationForUser(userType);

            // Assert
            Assert.Equal(config, result);
            _mockRedisService.Verify(x => x.GetAsync<TicketConfiguration>(cacheKey), Times.Once);
            _mockConfigRepo.Verify(x => x.GetConfigurationAsync(userType), Times.Once);
            _mockRedisService.Verify(x => x.SetAsync(
                cacheKey,
                config,
                It.Is<TimeSpan>(t => t == TimeSpan.FromMinutes(60))),
                Times.Once);
        }

        #endregion

        #region BuyTickets Tests

        [Fact]
        public async Task BuyTickets_WithEmptyList_ShouldReturnFailResult()
        {
            // Arrange
            var tickets = new List<Ticket>();
            var userName = "John Doe";
            var userId = Guid.NewGuid();
            var paymentType = "Card";

            // Act
            var result = await _service.BuyTickets(tickets, userName, userId, paymentType);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Ticket list cannot be empty.", result.Message);
            _mockTicketRepo.Verify(x => x.AddAsync(It.IsAny<Ticket>()), Times.Never);
            _mockPublishEndpoint.Verify(x => x.Publish(It.IsAny<CardInfoModel>(), default), Times.Never);
        }

        [Fact]
        public async Task BuyTickets_WithInvalidPaymentType_ShouldReturnFailResult()
        {
            // Arrange
            var tickets = new List<Ticket> { new() { Id = 1 } };
            var userName = "John Doe";
            var userId = Guid.NewGuid();
            var paymentType = "Invalid";

            // Act
            var result = await _service.BuyTickets(tickets, userName, userId, paymentType);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid payment type. Allowed values: Cash, Card.", result.Message);
            _mockTicketRepo.Verify(x => x.AddAsync(It.IsAny<Ticket>()), Times.Never);
            _mockPublishEndpoint.Verify(x => x.Publish(It.IsAny<CardInfoModel>(), default), Times.Never);
        }

        [Fact]
        public async Task BuyTickets_WithEmptyUserName_ShouldReturnFailResult()
        {
            // Arrange
            var tickets = new List<Ticket> 
            { 
                new() { Id = 1, UserName = "" } 
            };
            var userName = "John Doe";
            var userId = Guid.NewGuid();
            var paymentType = "Card";

            // Act
            var result = await _service.BuyTickets(tickets, userName, userId, paymentType);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("UserName cannot be empty", result.Message);
            _mockTicketRepo.Verify(x => x.AddAsync(It.IsAny<Ticket>()), Times.Never);
            _mockPublishEndpoint.Verify(x => x.Publish(It.IsAny<CardInfoModel>(), default), Times.Never);
        }

        [Fact]
        public async Task BuyTickets_WithPaymentTypeMismatch_ShouldReturnFailResult()
        {
            // Arrange
            var tickets = new List<Ticket> 
            { 
                new() { Id = 1, UserName = "John", PaymentType = "Cash" } 
            };
            var userName = "John Doe";
            var userId = Guid.NewGuid();
            var paymentType = "Card";

            // Act
            var result = await _service.BuyTickets(tickets, userName, userId, paymentType);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Payment type mismatch", result.Message);
            _mockTicketRepo.Verify(x => x.AddAsync(It.IsAny<Ticket>()), Times.Never);
            _mockPublishEndpoint.Verify(x => x.Publish(It.IsAny<CardInfoModel>(), default), Times.Never);
        }
        
        [Fact]
        public async Task BuyTickets_WithMultipleTickets_SomeFailingValidation_ShouldReturnPartialFailResult()
        {
            // Arrange
            var tickets = new List<Ticket> 
            { 
                new() { Id = 1, UserName = "John", Price = 100 },
                new() { Id = 2, UserName = "", Price = 50 } // Will fail validation
            };
            var userName = "John Doe";
            var userId = Guid.NewGuid();
            var paymentType = "Card";

            _mockTicketRepo
                .Setup(x => x.AddAsync(It.IsAny<Ticket>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.BuyTickets(tickets, userName, userId, paymentType);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("UserName cannot be empty", result.Message);
            _mockTicketRepo.Verify(x => x.AddAsync(It.IsAny<Ticket>()), Times.Once);
            _mockPublishEndpoint.Verify(x => x.Publish(It.IsAny<CardInfoModel>(), default), Times.Once);
        }

        #endregion

        #region DeleteTicket Tests

        [Fact]
        public async Task DeleteTicket_WhenSuccessful_ShouldRemoveCache()
        {
            // Arrange
            var ticketId = 1;
            var ticket = new Ticket { Id = ticketId, UserType = "Student" };

            _mockTicketRepo
                .Setup(x => x.GetTicketById(ticketId))
                .ReturnsAsync(ticket);

            _mockTicketRepo
                .Setup(x => x.DeleteAsync(ticket))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteTicket(ticketId);

            // Assert
            Assert.True(result.Success);
            _mockTicketRepo.Verify(x => x.DeleteAsync(ticket), Times.Once);
            _mockRedisService.Verify(x => x.RemoveAsync($"Tickets_Status_{ticket.UserType}"), Times.Once);
        }

        [Fact]
        public async Task DeleteTicket_WhenFails_ShouldReturnFailResult()
        {
            // Arrange
            var ticketId = 1;
            var ticket = new Ticket { Id = ticketId, UserType = "Student" };

            _mockTicketRepo
                .Setup(x => x.GetTicketById(ticketId))
                .ReturnsAsync(ticket);

            _mockTicketRepo
                .Setup(x => x.DeleteAsync(ticket))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeleteTicket(ticketId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Operation failed.", result.Message);
            _mockTicketRepo.Verify(x => x.DeleteAsync(ticket), Times.Once);
            _mockRedisService.Verify(x => x.RemoveAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region DeleteTicketConfiguration Tests

        [Fact]
        public async Task DeleteTicketConfiguration_WithEmptyUserType_ShouldReturnFailResult()
        {
            // Arrange
            var userType = "";

            // Act
            var result = await _service.DeleteTicketConfiguration(userType);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("UserType cannot be empty.", result.Message);
            _mockConfigRepo.Verify(x => x.DeleteAsync(It.IsAny<TicketConfiguration>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTicketConfiguration_WhenSuccessful_ShouldRemoveCache()
        {
            // Arrange
            var userType = "Student";
            var config = new TicketConfiguration { UserType = userType };

            _mockConfigRepo
                .Setup(x => x.GetConfigurationAsync(userType))
                .ReturnsAsync(config);

            _mockConfigRepo
                .Setup(x => x.DeleteAsync(config))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteTicketConfiguration(userType);

            // Assert
            Assert.True(result.Success);
            _mockConfigRepo.Verify(x => x.DeleteAsync(config), Times.Once);
            _mockRedisService.Verify(x => x.RemoveAsync($"TicketConfig_{userType}"), Times.Once);
            _mockRedisService.Verify(x => x.RemoveAsync("All_Ticket_Configurations"), Times.Once);
        }

        #endregion

        #region AddConfiguration Tests

        [Fact]
        public async Task AddConfiguration_WithEmptyUserType_ShouldReturnFailResult()
        {
            // Arrange
            var config = new TicketConfiguration { UserType = "", Price = 100 };

            // Act
            var result = await _service.AddConfiguration(config);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("TicketConfiguration user type cannot be empty.", result.Message);
            _mockConfigRepo.Verify(x => x.AddAsync(It.IsAny<TicketConfiguration>()), Times.Never);
        }

        [Fact]
        public async Task AddConfiguration_WhenSuccessful_ShouldRemoveCache()
        {
            // Arrange
            var config = new TicketConfiguration { UserType = "Student", Price = 100 };

            _mockConfigRepo
                .Setup(x => x.AddAsync(config))
                .ReturnsAsync(true);

            // Act
            var result = await _service.AddConfiguration(config);

            // Assert
            Assert.True(result.Success);
            _mockConfigRepo.Verify(x => x.AddAsync(config), Times.Once);
            _mockRedisService.Verify(x => x.RemoveAsync($"TicketConfig_{config.UserType}"), Times.Once);
            _mockRedisService.Verify(x => x.RemoveAsync("All_Ticket_Configurations"), Times.Once);
        }

        #endregion

        #region EditConfiguration Tests

        [Fact]
        public async Task EditConfiguration_WhenSuccessful_ShouldRemoveCache()
        {
            // Arrange
            var oldUserType = "Student";
            var newUserType = "Adult";
            var price = 150.0;
            var duration = 2;
            
            var currentConfig = new TicketConfiguration 
            { 
                UserType = oldUserType, 
                Price = 100, 
                DurationInHours = 1 
            };

            _mockConfigRepo
                .Setup(x => x.GetConfigurationAsync(oldUserType))
                .ReturnsAsync(currentConfig);

            _mockConfigRepo
                .Setup(x => x.EditAsync(It.IsAny<TicketConfiguration>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.EditConfiguration(oldUserType, price, duration, newUserType);

            // Assert
            Assert.True(result.Success);
            _mockConfigRepo.Verify(x => x.EditAsync(It.Is<TicketConfiguration>(
                c => c.UserType == newUserType && 
                     c.Price == price && 
                     c.DurationInHours == duration)), 
                Times.Once);
            
            _mockRedisService.Verify(x => x.RemoveAsync($"TicketConfig_{oldUserType}"), Times.Once);
            _mockRedisService.Verify(x => x.RemoveAsync($"TicketConfig_{newUserType}"), Times.Once);
            _mockRedisService.Verify(x => x.RemoveAsync("All_Ticket_Configurations"), Times.Once);
        }

        #endregion

        #region DeleteExpiredTicket Tests

        [Fact]
        public async Task DeleteExpiredTicket_WhenSuccessful_ShouldDeleteTicket()
        {
            // Arrange
            var ticketId = 1;
            var ticket = new Ticket { Id = ticketId };

            _mockTicketRepo
                .Setup(x => x.GetTicketById(ticketId))
                .ReturnsAsync(ticket);

            _mockTicketRepo
                .Setup(x => x.DeleteAsync(ticket))
                .ReturnsAsync(true);

            // Act
            await _service.DeleteExpiredTicket(ticketId);

            // Assert
            _mockTicketRepo.Verify(x => x.GetTicketById(ticketId), Times.Once);
            _mockTicketRepo.Verify(x => x.DeleteAsync(ticket), Times.Once);
        }

        [Fact]
        public async Task DeleteExpiredTicket_WhenThrowsException_ShouldHandleGracefully()
        {
            // Arrange
            var ticketId = 1;

            _mockTicketRepo
                .Setup(x => x.GetTicketById(ticketId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await _service.DeleteExpiredTicket(ticketId); // Should not throw
            
            _mockTicketRepo.Verify(x => x.GetTicketById(ticketId), Times.Once);
            _mockTicketRepo.Verify(x => x.DeleteAsync(It.IsAny<Ticket>()), Times.Never);
        }

        #endregion
    }
}