using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Cryptography;
using System.Text;
using Versum;
using Versum.Context;
using Versum.Dtos;
using Versum.Services;

namespace VersumTestProject.ServicesTest
{
    public class AuthServiceTests: IDisposable
    {

        private readonly ApplicationDbContext _context;
        private readonly ApplicationDbContext _assertContext;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly AuthService _authService;

        private const string TestUsername = "vixy";
        private const string TestEmail = "vixy@test.com";
        private const string TestPassword = "SecurePassword123";
        const string ExistingUser = "existing_user";
        const string ExistingEmail = "existing_user";


        private const string ValidRawToken = "valid_token_123";
        private const string ExpiredRawToken = "expired_token_123";

        // SET UP
        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(c => c["AppSettings:BaseUrl"]).Returns("https://localhost:7014");

            _context = new ApplicationDbContext(options);
            _authService = new AuthService(_context, _emailServiceMock.Object, _configurationMock.Object);
            _assertContext = new ApplicationDbContext(options);


            TemporaryTemplate();

        }
        private void TemporaryTemplate()
        {
            var templatesDir = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
            if (!Directory.Exists(templatesDir))
            {
                Directory.CreateDirectory(templatesDir);
            }
            File.WriteAllText(
                Path.Combine(templatesDir, "ConfRegistrationTemplate.html"),
                "Привіт, {Username}! Посилання: {confirmLink}"
            );
        }
       
        private string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return Convert.ToBase64String(bytes);
            }
        }

        // TEARDOWN
        public void Dispose()
        {
            _context.Dispose();
            _assertContext.Dispose();

            var templatesDir = Path.Combine(AppContext.BaseDirectory, "Templates");
            if (Directory.Exists(templatesDir))
            {
                Directory.Delete(templatesDir, true);
            }
        }

        [Fact]
        public async Task RegisterAsync_WhenDataIsValid_SuccessfullyRegistersUserAndSendsEmail()
        {
            // Arrange
            var dto = new RegisterDto { Username = TestUsername, Email = TestEmail, Password = TestPassword };

            // Act
            var (success, error, field) = await _authService.RegisterAsync(dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);
            Assert.Null(field);

            // checks if user is saved in database
            var userInDb = await _assertContext.Users.FirstOrDefaultAsync(u => u.Email == TestEmail);

            Assert.NotNull(userInDb);
            Assert.Equal(TestUsername, userInDb.Username);

            // checks if password is hashed
            Assert.NotEqual(TestPassword, userInDb.PasswordHash);

            // checks token
            Assert.NotNull(userInDb.EmailConfirmationTokenHash);
            Assert.True(userInDb.EmailTokenExpiryDate > DateTime.UtcNow);

            // checks email sending with use of Email Mock
            // У тестах ви просто робите так:
           
            _emailServiceMock.Verify(
                x => x.SendEmailAsync(
                    TestEmail,
                    "Підтвердження реєстрації — Versum",
                    It.Is<string>(body => body.Contains("https://localhost:7014/api/Auth/confirm-email") && body.Contains(TestUsername))
                ),
                Times.Once
            );
        }



        [Fact]
        public async Task RegisterAsync_WhenUsernameAlreadyExists_ReturnsError()
        {
            // Arrange

            _context.Users.Add(new User { Id = 1, Username = ExistingUser, Email = "old@test.com", PasswordHash = "123yuyuyyuy" });
            await _context.SaveChangesAsync();
            var dto = new RegisterDto { Username = ExistingUser, Email = "new@test.com", Password = "Password123!" };

            // Act
            var (success, error, field) = await _authService.RegisterAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Цей нікнейм вже існує", error);
            Assert.Equal("username", field);
        }

        [Fact]
        public async Task RegisterAsync_WhenEmailAlreadyExists_ReturnsError()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Username = "old_user", Email = ExistingEmail, PasswordHash = "ere998rer" });
            await _context.SaveChangesAsync();
            var dto = new RegisterDto { Username = "new_user", Email = ExistingEmail, Password = "Password123!" };

            // Act
            var (success, error, field) = await _authService.RegisterAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Цей імейл вже існує", error);
            Assert.Equal("email", field);
        }



        [Fact]
        public async Task ConfirmEmailAsync_WithValidAndActiveToken_ReturnsSuccessAndUpdatesUser()
        {
            // Arrange
            string rawToken = ValidRawToken;
            string tokenHash = ComputeSha256Hash(rawToken);

            var user = new User
            {
                Id = 1,
                Username = TestUsername,
                Email = TestEmail,
                PasswordHash = "hashed_pw",
                IsEmailConfirmed = false,
                EmailConfirmationTokenHash = tokenHash,
                EmailTokenExpiryDate = DateTime.UtcNow.AddHours(24)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _authService.ConfirmEmailAsync(rawToken);

            // Assert
            Assert.True(success);
            Assert.Null(error);

            var updatedUser = await _assertContext.Users.FirstAsync(u => u.Id == 1);

            Assert.True(updatedUser.IsEmailConfirmed);
            Assert.Null(updatedUser.EmailConfirmationTokenHash);
            Assert.Null(updatedUser.EmailTokenExpiryDate);
        }


        [Fact]
        public async Task ConfirmEmailAsync_WithInvalidToken_ReturnsFalseAndError()
        {
            // Arrange
            string rawTokenInDb = "correct_token";
            string tokenHash = ComputeSha256Hash(rawTokenInDb);

            var user = new User
            {
                Id = 1,
                Username = TestUsername,
                Email = TestEmail,
                PasswordHash = "hashed_pw",
                IsEmailConfirmed = false,
                EmailConfirmationTokenHash = tokenHash,
                EmailTokenExpiryDate = DateTime.UtcNow.AddHours(2)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var authService = new global::Versum.Services.AuthService(_context, _emailServiceMock.Object, _configurationMock.Object);

            // Act
            var (success, error) = await authService.ConfirmEmailAsync("wrong_token");

            // Assert
            Assert.False(success);
            Assert.Equal("Посилання недійсне або термін дії вичерпано", error);

            var notUpdatedUser = await _assertContext.Users.FirstAsync(u => u.Id == 1);
            Assert.False(notUpdatedUser.IsEmailConfirmed);
            Assert.NotNull(notUpdatedUser.EmailConfirmationTokenHash);
        }

        [Fact]
        public async Task ConfirmEmailAsync_WithExpiredToken_ReturnsFalseAndError()
        {
            // Arrange

            string rawToken = ExpiredRawToken;
            string tokenHash = ComputeSha256Hash(rawToken);

            var user = new User
            {
                Id = 1,
                Username = TestUsername,
                Email = TestEmail,
                PasswordHash = "hashed_pw",
                IsEmailConfirmed = false,
                EmailConfirmationTokenHash = tokenHash,
                EmailTokenExpiryDate = DateTime.UtcNow.AddMinutes(-10) // Time ends 10 min ago
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _authService.ConfirmEmailAsync(rawToken);

            // Assert
            Assert.False(success);
            Assert.Equal("Посилання недійсне або термін дії вичерпано", error);

            var notUpdatedUser = await _assertContext.Users.FirstAsync(u => u.Id == 1);
            Assert.False(notUpdatedUser.IsEmailConfirmed);
            Assert.NotNull(notUpdatedUser.EmailConfirmationTokenHash);
        }
    }
}



