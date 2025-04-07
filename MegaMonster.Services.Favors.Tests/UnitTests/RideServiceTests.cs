using FluentAssertions;
using MegaMonster.Services.Favors.Application.Ride;
using MegaMonster.Services.Favors.Core.Ride;
using MegaMonster.Services.Favors.Infrastructure.Redis;
using Moq;

namespace MegaMonster.Services.Favors.Tests.UnitTests
{
    public class RideServiceTests
    {
        private readonly Mock<IRideRepository> _mockRideRepository;
        private readonly Mock<IRedisService> _mockRedisService;
        private readonly RideService _rideService;
        
        private Ride CreateValidRide(string name = "Test Ride", int categoryId = 1)
        {
            return new Ride(name)
            {
                CategoryId = categoryId,
                ClientStatus = "Active",
                Rating = 4.5,
                Image = "ride-image.jpg"
            };
        }

        public RideServiceTests()
        {
            _mockRideRepository = new Mock<IRideRepository>();
            _mockRedisService = new Mock<IRedisService>();
            _rideService = new RideService(_mockRideRepository.Object, _mockRedisService.Object);
        }

        #region GetAllRides Tests

        [Fact]
        public async Task GetAllRides_WhenCacheExists_ShouldReturnCachedData()
        {
            // Arrange
            var cachedRides = new System.Collections.Generic.List<Ride>
            {
                CreateValidRide("Test Ride 1"),
                CreateValidRide("Test Ride 2")
            };
            cachedRides[0].Id = 1;
            cachedRides[1].Id = 2;

            _mockRedisService.Setup(s => s.GetAsync<System.Collections.Generic.IEnumerable<Ride>>("All_Rides"))
                .ReturnsAsync(cachedRides);

            // Act
            var result = await _rideService.GetAllRides();

            // Assert
            result.Should().BeEquivalentTo(cachedRides);
            _mockRideRepository.Verify(r => r.GetAll(), Times.Never);
        }

        [Fact]
        public async Task GetAllRides_WhenCacheDoesNotExist_ShouldFetchFromRepositoryAndCache()
        {
            // Arrange
            var rides = new List<Ride>
            {
                CreateValidRide("Test Ride 1"),
                CreateValidRide("Test Ride 2")
            };
            rides[0].Id = 1;
            rides[1].Id = 2;

            _mockRedisService.Setup(s => s.GetAsync<IEnumerable<Ride>>("All_Rides"))
                .ReturnsAsync((IEnumerable<Ride>)null);
            _mockRideRepository.Setup(r => r.GetAll())
                .ReturnsAsync(rides);

            // Act
            var result = await _rideService.GetAllRides();

            // Assert
            result.Should().BeEquivalentTo(rides);
            _mockRedisService.Verify(s => s.SetAsync(
                    It.Is<string>(key => key == "All_Rides"),
                    It.Is<object>(val => val.Equals(rides)),
                    It.Is<TimeSpan>(t => t == TimeSpan.FromHours(1))), 
                Times.Once);
        }

        #endregion

        #region GetRideById Tests

        [Fact]
        public async Task GetRideById_WhenCacheExists_ShouldReturnCachedData()
        {
            // Arrange
            var rideId = 1;
            var cachedRide = CreateValidRide();
            cachedRide.Id = rideId;

            _mockRedisService.Setup(s => s.GetAsync<Ride>($"Ride_{rideId}"))
                .ReturnsAsync(cachedRide);

            // Act
            var result = await _rideService.GetRideById(rideId);

            // Assert
            result.Should().BeEquivalentTo(cachedRide);
            _mockRideRepository.Verify(r => r.GetRideById(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetRideById_WhenCacheDoesNotExist_ShouldFetchFromRepositoryAndCache()
        {
            // Arrange
            var rideId = 1;
            var ride = CreateValidRide();
            ride.Id = rideId;

            _mockRedisService.Setup(s => s.GetAsync<Ride>($"Ride_{rideId}"))
                .ReturnsAsync((Ride)null);
            _mockRideRepository.Setup(r => r.GetRideById(rideId))
                .ReturnsAsync(ride);

            // Act
            var result = await _rideService.GetRideById(rideId);

            // Assert
            result.Should().BeEquivalentTo(ride);
            _mockRedisService.Verify(s => s.SetAsync($"Ride_{rideId}", ride, It.IsAny<TimeSpan>()), Times.Once);
        }

        #endregion

        #region GetRideByName Tests

        [Fact]
        public async Task GetRideByName_WhenCacheExists_ShouldReturnCachedData()
        {
            // Arrange
            var rideName = "Test Ride";
            var cachedRide = CreateValidRide(rideName);
            cachedRide.Id = 1;

            _mockRedisService.Setup(s => s.GetAsync<Ride>($"Ride_{rideName}"))
                .ReturnsAsync(cachedRide);

            // Act
            var result = await _rideService.GetRideByName(rideName);

            // Assert
            result.Should().BeEquivalentTo(cachedRide);
            _mockRideRepository.Verify(r => r.GetRideByName(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetRideByName_WhenCacheDoesNotExist_ShouldFetchFromRepositoryAndCache()
        {
            // Arrange
            var rideName = "Test Ride";
            var ride = CreateValidRide(rideName);
            ride.Id = 1;

            _mockRedisService.Setup(s => s.GetAsync<Ride>($"Ride_{rideName}"))
                .ReturnsAsync((Ride)null);
            _mockRideRepository.Setup(r => r.GetRideByName(rideName))
                .ReturnsAsync(ride);

            // Act
            var result = await _rideService.GetRideByName(rideName);

            // Assert
            result.Should().BeEquivalentTo(ride);
            _mockRedisService.Verify(s => s.SetAsync($"Ride_{rideName}", ride, It.IsAny<TimeSpan>()), Times.Once);
        }

        #endregion

        #region AddRide Tests

        [Fact]
        public async Task AddRide_WithEmptyName_ShouldReturnFailResult()
        {
            // Arrange
            var ride = new Ride("")
            {
                Image = "test.jpg",
                ClientStatus = "Active"
            };

            // Act
            var result = await _rideService.AddRide(ride);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Ride name cannot be empty.");
            _mockRideRepository.Verify(r => r.AddAsync(It.IsAny<Ride>()), Times.Never);
        }

        [Fact]
        public async Task AddRide_WithExistingName_ShouldReturnFailResult()
        {
            // Arrange
            var rideName = "Existing Ride";
            var ride = CreateValidRide(rideName);
            
            _mockRideRepository.Setup(r => r.GetRideByName(rideName))
                .ReturnsAsync(CreateValidRide(rideName));

            // Act
            var result = await _rideService.AddRide(ride);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be($"Ride with name '{rideName}' already exists.");
            _mockRideRepository.Verify(r => r.AddAsync(It.IsAny<Ride>()), Times.Never);
        }

        [Fact]
        public async Task AddRide_WhenRepositoryFailsToAdd_ShouldReturnFailResult()
        {
            // Arrange
            var ride = CreateValidRide();
            
            _mockRideRepository.Setup(r => r.GetRideByName(ride.Name))
                .ReturnsAsync((Ride)null);
            _mockRideRepository.Setup(r => r.AddAsync(ride))
                .ReturnsAsync(false);

            // Act
            var result = await _rideService.AddRide(ride);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error adding ride.");
            _mockRedisService.Verify(s => s.RemoveAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AddRide_WhenSuccessful_ShouldReturnSuccessAndInvalidateCache()
        {
            // Arrange
            var ride = CreateValidRide();
            
            _mockRideRepository.Setup(r => r.GetRideByName(ride.Name))
                .ReturnsAsync((Ride)null);
            _mockRideRepository.Setup(r => r.AddAsync(ride))
                .ReturnsAsync(true);

            // Act
            var result = await _rideService.AddRide(ride);

            // Assert
            result.Success.Should().BeTrue();
            _mockRedisService.Verify(s => s.RemoveAsync("All_Rides"), Times.Once);
            _mockRedisService.Verify(s => s.RemoveAsync($"Ride_{ride.Name}"), Times.Once);
        }

        #endregion

        #region EditRide Tests

        [Fact]
        public async Task EditRide_WithEmptyNewName_ShouldReturnFailResult()
        {
            // Arrange
            var currentName = "Current Ride";
            var newName = "";

            // Act
            var result = await _rideService.EditRide(currentName, newName, 1, "Active", 4.5);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("New ride name cannot be empty.");
            _mockRideRepository.Verify(r => r.EditAsync(It.IsAny<Ride>()), Times.Never);
        }

        [Fact]
        public async Task EditRide_WithNonExistingCurrentName_ShouldReturnFailResult()
        {
            // Arrange
            var currentName = "Non-Existing Ride";
            var newName = "New Ride";
            
            _mockRideRepository.Setup(r => r.GetRideByName(currentName))
                .ReturnsAsync((Ride)null);

            // Act
            var result = await _rideService.EditRide(currentName, newName, 1, "Active", 4.5);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be($"Ride '{currentName}' not found.");
            _mockRideRepository.Verify(r => r.EditAsync(It.IsAny<Ride>()), Times.Never);
        }

        [Fact]
        public async Task EditRide_WithAlreadyExistingNewName_ShouldReturnFailResult()
        {
            // Arrange
            var currentName = "Current Ride";
            var newName = "Existing Ride";
            var currentRide = CreateValidRide(currentName);
            currentRide.Id = 1;
            
            var existingRide = CreateValidRide(newName);
            existingRide.Id = 2;
            
            _mockRideRepository.Setup(r => r.GetRideByName(currentName))
                .ReturnsAsync(currentRide);
            _mockRideRepository.Setup(r => r.GetRideByName(newName))
                .ReturnsAsync(existingRide);

            // Act
            var result = await _rideService.EditRide(currentName, newName, 1, "Active", 4.5);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be($"Ride with name '{newName}' already exists.");
            _mockRideRepository.Verify(r => r.EditAsync(It.IsAny<Ride>()), Times.Never);
        }

        [Fact]
        public async Task EditRide_WhenRepositoryFailsToEdit_ShouldReturnFailResult()
        {
            // Arrange
            var currentName = "Current Ride";
            var newName = "New Ride";
            var currentRide = CreateValidRide(currentName);
            currentRide.Id = 1;
            
            _mockRideRepository.Setup(r => r.GetRideByName(currentName))
                .ReturnsAsync(currentRide);
            _mockRideRepository.Setup(r => r.GetRideByName(newName))
                .ReturnsAsync((Ride)null);
            _mockRideRepository.Setup(r => r.EditAsync(It.IsAny<Ride>()))
                .ReturnsAsync(false);

            // Act
            var result = await _rideService.EditRide(currentName, newName, 2, "Inactive", 3.5);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error updating ride.");
            _mockRedisService.Verify(s => s.RemoveAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EditRide_WhenSuccessful_ShouldUpdateRideAndInvalidateCache()
        {
            // Arrange
            var currentName = "Current Ride";
            var newName = "New Ride";
            var categoryId = 2;
            var status = "Inactive";
            var rating = 3.5;
            
            var currentRide = CreateValidRide(currentName);
            currentRide.Id = 1;
            currentRide.CategoryId = 1;
            currentRide.ClientStatus = "Active";
            currentRide.Rating = 4.0;
            
            _mockRideRepository.Setup(r => r.GetRideByName(currentName))
                .ReturnsAsync(currentRide);
            _mockRideRepository.Setup(r => r.GetRideByName(newName))
                .ReturnsAsync((Ride)null);
            _mockRideRepository.Setup(r => r.EditAsync(It.Is<Ride>(ride => 
                ride.Name == newName && 
                ride.CategoryId == categoryId && 
                ride.ClientStatus == status && 
                ride.Rating == rating)))
                .ReturnsAsync(true);

            // Act
            var result = await _rideService.EditRide(currentName, newName, categoryId, status, rating);

            // Assert
            result.Success.Should().BeTrue();
            _mockRedisService.Verify(s => s.RemoveAsync("All_Rides"), Times.Once);
            _mockRedisService.Verify(s => s.RemoveAsync($"Ride_{currentName}"), Times.Once);
            _mockRedisService.Verify(s => s.RemoveAsync($"Ride_{newName}"), Times.Once);
            
            // Verify ride properties were updated correctly
            currentRide.Name.Should().Be(newName);
            currentRide.CategoryId.Should().Be(categoryId);
            currentRide.ClientStatus.Should().Be(status);
            currentRide.Rating.Should().Be(rating);
            // Image should remain unchanged
            currentRide.Image.Should().Be("ride-image.jpg");
        }

        #endregion

        #region DeleteRide Tests

        [Fact]
        public async Task DeleteRide_WithNonExistingName_ShouldReturnFailResult()
        {
            // Arrange
            var rideName = "Non-Existing Ride";
            _mockRideRepository.Setup(r => r.GetRideByName(rideName))
                .ReturnsAsync((Ride)null);

            // Act
            var result = await _rideService.DeleteRide(rideName);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be($"Ride '{rideName}' not found.");
            _mockRideRepository.Verify(r => r.DeleteAsync(It.IsAny<Ride>()), Times.Never);
        }

        [Fact]
        public async Task DeleteRide_WhenRepositoryFailsToDelete_ShouldReturnFailResult()
        {
            // Arrange
            var rideName = "Test Ride";
            var ride = CreateValidRide(rideName);
            ride.Id = 1;
            
            _mockRideRepository.Setup(r => r.GetRideByName(rideName))
                .ReturnsAsync(ride);
            _mockRideRepository.Setup(r => r.DeleteAsync(ride))
                .ReturnsAsync(false);

            // Act
            var result = await _rideService.DeleteRide(rideName);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Error deleting ride.");
            _mockRedisService.Verify(s => s.RemoveAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteRide_WhenSuccessful_ShouldReturnSuccessAndInvalidateCache()
        {
            // Arrange
            var rideName = "Test Ride";
            var ride = CreateValidRide(rideName);
            ride.Id = 1;
            
            _mockRideRepository.Setup(r => r.GetRideByName(rideName))
                .ReturnsAsync(ride);
            _mockRideRepository.Setup(r => r.DeleteAsync(ride))
                .ReturnsAsync(true);

            // Act
            var result = await _rideService.DeleteRide(rideName);

            // Assert
            result.Success.Should().BeTrue();
            _mockRedisService.Verify(s => s.RemoveAsync("All_Rides"), Times.Once);
            _mockRedisService.Verify(s => s.RemoveAsync($"Ride_{rideName}"), Times.Once);
        }

        #endregion

        #region GetOrSetCache Tests

        [Fact]
        public async Task GetOrSetCache_WhenCacheExists_ShouldReturnCachedDataWithoutCallingGetData()
        {
            // Arrange
            var key = "test_key";
            var cachedData = CreateValidRide();
            cachedData.Id = 1;
    
            var getDataCalled = false;
    
            _mockRedisService.Setup(s => s.GetAsync<Ride>(key))
                .ReturnsAsync(cachedData);
    
            Func<Task<Ride>> getData = () => 
            {
                getDataCalled = true;
                return Task.FromResult(CreateValidRide("Different Ride"));
            };

            // Use reflection to access the private method
            var methodInfo = typeof(RideService).GetMethod("GetOrSetCache", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    
            // Create a closed generic method with the concrete type Ride
            var closedMethod = methodInfo.MakeGenericMethod(typeof(Ride));
    
            // Act
            var result = await (Task<Ride>)closedMethod.Invoke(_rideService, new object[] 
            { 
                key, 
                getData, 
                null 
            });

            // Assert
            result.Should().BeEquivalentTo(cachedData);
            getDataCalled.Should().BeFalse();
            _mockRedisService.Verify(s => s.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task GetOrSetCache_WhenCacheDoesNotExist_ShouldCallGetDataAndCacheResult()
        {
            // Arrange
            var key = "test_key";
            var data = CreateValidRide();
            data.Id = 1;
    
            var getDataCalled = false;
    
            _mockRedisService.Setup(s => s.GetAsync<Ride>(key))
                .ReturnsAsync((Ride)null);
    
            Func<Task<Ride>> getData = () => 
            {
                getDataCalled = true;
                return Task.FromResult(data);
            };

            // Get the generic method definition
            var methodInfo = typeof(RideService).GetMethod("GetOrSetCache", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    
            // Create a closed generic method with the concrete type Ride
            var closedMethod = methodInfo?.MakeGenericMethod(typeof(Ride));
    
            // Act
            var result = await (Task<Ride>)closedMethod.Invoke(_rideService, new object[] 
            { 
                key, 
                getData, 
                TimeSpan.FromMinutes(30) 
            });

            // Assert
            result.Should().BeEquivalentTo(data);
            getDataCalled.Should().BeTrue();
            _mockRedisService.Verify(s => s.SetAsync(key, data, TimeSpan.FromMinutes(30)), Times.Once);
        }

        [Fact]
        public async Task GetOrSetCache_WhenDataIsNull_ShouldNotCacheResult()
        {
            // Arrange
            var key = "test_key";
    
            _mockRedisService.Setup(s => s.GetAsync<Ride>(key))
                .ReturnsAsync((Ride)null);
    
            Func<Task<Ride>> getData = () => Task.FromResult<Ride>(null);

            // Use reflection to access the private method
            var methodInfo = typeof(RideService).GetMethod("GetOrSetCache", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    
            // Create a closed generic method with the concrete type Ride
            var closedMethod = methodInfo.MakeGenericMethod(typeof(Ride));
    
            // Act
            var result = await (Task<Ride>)closedMethod.Invoke(_rideService, new object[] 
            { 
                key, 
                getData, 
                null 
            });

            // Assert
            result.Should().BeNull();
            _mockRedisService.Verify(s => s.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Fact]
        public async Task GetOrSetCache_WithDefaultExpiration_ShouldUse60MinuteExpiration()
        {
            // Arrange
            var key = "test_key";
            var data = CreateValidRide();
            data.Id = 1;
    
            _mockRedisService.Setup(s => s.GetAsync<Ride>(key))
                .ReturnsAsync((Ride)null);
    
            Func<Task<Ride>> getData = () => Task.FromResult(data);

            // Act
            var result = await InvokeGetOrSetCache<Ride>(key, getData, null);

            // Assert
            result.Should().BeEquivalentTo(data);
            _mockRedisService.Verify(s => s.SetAsync(key, data, TimeSpan.FromMinutes(60)), Times.Once);
        }

        #endregion

        #region ProcessChange Tests

        [Fact]
        public async Task ProcessChange_WithSingleRideName_ShouldInvalidateCorrectCacheEntries()
        {
            // Arrange
            var rideName = "Test Ride";
            
            var method = typeof(RideService).GetMethod("ProcessChange", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            await (Task)method.Invoke(_rideService, new object[] { new string[] { rideName } });

            // Assert
            _mockRedisService.Verify(s => s.RemoveAsync("All_Rides"), Times.Once);
            _mockRedisService.Verify(s => s.RemoveAsync($"Ride_{rideName}"), Times.Once);
        }

        [Fact]
        public async Task ProcessChange_WithMultipleRideNames_ShouldInvalidateAllCacheEntries()
        {
            // Arrange
            var rideName1 = "Test Ride 1";
            var rideName2 = "Test Ride 2";
            
            var method = typeof(RideService).GetMethod("ProcessChange", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            // Act
            await (Task)method.Invoke(_rideService, new object[] { new string[] { rideName1, rideName2 } });

            // Assert
            _mockRedisService.Verify(s => s.RemoveAsync("All_Rides"), Times.Once);
            _mockRedisService.Verify(s => s.RemoveAsync($"Ride_{rideName1}"), Times.Once);
            _mockRedisService.Verify(s => s.RemoveAsync($"Ride_{rideName2}"), Times.Once);
        }

        #endregion
        
        private async Task<T> InvokeGetOrSetCache<T>(string key, Func<Task<T>> getData, TimeSpan? expiry)
        {
            var methodInfo = typeof(RideService).GetMethod("GetOrSetCache", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
    
            var closedMethod = methodInfo.MakeGenericMethod(typeof(T));
    
            return await (Task<T>)closedMethod.Invoke(_rideService, new object[] 
            { 
                key, 
                getData, 
                expiry 
            });
        }
    }
}