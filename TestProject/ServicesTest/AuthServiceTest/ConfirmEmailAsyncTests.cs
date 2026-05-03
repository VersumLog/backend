using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Security.Cryptography;
using System.Text;
using Versum;
using Versum.Context;
using Versum.Services;

namespace VersumTestProject.ServicesTest.AuthServiceTest
{
    public class ConfirmEmailAsyncTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;

        public ConfirmEmailAsyncTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

           
            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock = new Mock<IConfiguration>();
        }

        // Helping method for generation SHA256 
        private string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return Convert.ToBase64String(bytes);
            }
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithValidAndActiveToken_ReturnsSuccessAndUpdatesUser()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            string rawToken = "valid_token_123";
            string tokenHash = ComputeSha256Hash(rawToken);

            var user = new User
            {
                Id = 1,
                Username = "vixy",
                Email = "vixy@test.com",
                PasswordHash = "hashed_pw",
                IsEmailConfirmed = false,
                EmailConfirmationTokenHash = tokenHash,
                EmailTokenExpiryDate = DateTime.UtcNow.AddHours(24) 
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, _emailServiceMock.Object, _configurationMock.Object);

            // Act
            var (success, error) = await authService.ConfirmEmailAsync(rawToken);

            // Assert
            Assert.True(success);
            Assert.Null(error);

           
            using var assertContext = new ApplicationDbContext(_options);
            var updatedUser = await assertContext.Users.FirstAsync(u => u.Id == 1);

            Assert.True(updatedUser.IsEmailConfirmed);
            Assert.Null(updatedUser.EmailConfirmationTokenHash);
            Assert.Null(updatedUser.EmailTokenExpiryDate);
        }


        [Fact]
        public async Task ConfirmEmailAsync_WithInvalidToken_ReturnsFalseAndError()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            string rawTokenInDb = "correct_token";
            string tokenHash = ComputeSha256Hash(rawTokenInDb);

            var user = new User
            {
                Id = 1,
                Username = "vixy",
                Email = "vixy@test.com",
                PasswordHash = "hashed_pw",
                IsEmailConfirmed = false,
                EmailConfirmationTokenHash = tokenHash,
                EmailTokenExpiryDate = DateTime.UtcNow.AddHours(2)
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var authService = new global::Versum.Services.AuthService(context, _emailServiceMock.Object, _configurationMock.Object);

            // Act
            var (success, error) = await authService.ConfirmEmailAsync("wrong_token");

            // Assert
            Assert.False(success);
            Assert.Equal("Посилання недійсне або термін дії вичерпано", error);

           
            using var assertContext = new ApplicationDbContext(_options);
            var notUpdatedUser = await assertContext.Users.FirstAsync(u => u.Id == 1);
            Assert.False(notUpdatedUser.IsEmailConfirmed);
            Assert.NotNull(notUpdatedUser.EmailConfirmationTokenHash);
        }
        [Fact]
        public async Task ConfirmEmailAsync_WithExpiredToken_ReturnsFalseAndError()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            string rawToken = "expired_token_123";
            string tokenHash = ComputeSha256Hash(rawToken);

            var user = new User
            {
                Id = 1,
                Username = "vixy",
                Email = "vixy@test.com",
                PasswordHash = "hashed_pw",
                IsEmailConfirmed = false,
                EmailConfirmationTokenHash = tokenHash,
                EmailTokenExpiryDate = DateTime.UtcNow.AddMinutes(-10) // Time ends 10 min ago
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();

            var authService = new global::Versum.Services.AuthService(context, _emailServiceMock.Object, _configurationMock.Object);

            // Act
            var (success, error) = await authService.ConfirmEmailAsync(rawToken);

            // Assert
            Assert.False(success);
            Assert.Equal("Посилання недійсне або термін дії вичерпано", error);

           
            using var assertContext = new ApplicationDbContext(_options);
            var notUpdatedUser = await assertContext.Users.FirstAsync(u => u.Id == 1);
            Assert.False(notUpdatedUser.IsEmailConfirmed);
            Assert.NotNull(notUpdatedUser.EmailConfirmationTokenHash);
        }
    
  }
}
