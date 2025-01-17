using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using TheWebApplication.Controllers;
using ClassLibrary;
using ClassLibrary.Models;
using ClassLibrary.DtoModels.Screen;
using ClassLibrary.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IPasswordHasher<Admin>> _mockPasswordHasher;
        private readonly Mock<ILogger<AuthController>> _mockLogger;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly ClassDBContext _context;

        public AuthControllerTests()
        {
            var options = new DbContextOptionsBuilder<ClassDBContext>()
                .UseInMemoryDatabase(databaseName: "TestAuthDb_" + Guid.NewGuid().ToString())
                .Options;
            _context = new ClassDBContext(options);

            _mockPasswordHasher = new Mock<IPasswordHasher<Admin>>();
            _mockLogger = new Mock<ILogger<AuthController>>();
            _mockConfiguration = new Mock<IConfiguration>();

            _mockConfiguration.Setup(x => x["JwtSettings:Key"]).Returns("your-test-secret-key-with-minimum-16-characters");
            _mockConfiguration.Setup(x => x["JwtSettings:Issuer"]).Returns("test-issuer");
            _mockConfiguration.Setup(x => x["JwtSettings:Audience"]).Returns("test-audience");
        }

        [Fact]
        public async Task ScreenLogin_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var screen = new Screen
            {
                Id = 1,
                Name = "TestScreen",
                Description = "Test Screen Description",
                ScreenType = "LCD",
                StatusMessage = "Active",
                LocationId = 1,
                DepartmentId = 1,
                MACAddress = "00:11:22:33:44:55"
            };

            await _context.Screens.AddAsync(screen);
            await _context.SaveChangesAsync();

            // Setup mock logger to capture errors
            var logMessages = new List<string>();
            _mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
                .Callback<LogLevel, EventId, object, Exception, Delegate>((level, id, state, ex, formatter) =>
                {
                    logMessages.Add($"{level}: {state}");
                });

            var controller = new AuthController(
                _context, 
                _mockConfiguration.Object,
                _mockPasswordHasher.Object,
                _mockLogger.Object
            );

            var loginDto = new LoginScreenDto
            {
                Password = "TestScreen"
            };

            // Act
            var result = await controller.ScreenLogin(loginDto);

            // Debug output
            Console.WriteLine("Log Messages:");
            foreach (var message in logMessages)
            {
                Console.WriteLine(message);
            }

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            var response = Assert.IsType<ApiResponse<string>>(objectResult.Value);
            
            if (!response.Success)
            {
                Console.WriteLine($"Response Message: {response.Message}");
            }
            
            Assert.Equal(200, objectResult.StatusCode);
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
        }
    }
} 