using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Versum;
using Versum.Context;
using Versum.Dtos;
using Versum.Services;


namespace VersumTestProject.ServicesTest.AuthServiceTest
{
    public class RegisterAsyncTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;

        public RegisterAsyncTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            _emailServiceMock = new Mock<IEmailService>();
            _configurationMock = new Mock<IConfiguration>();

           
            _configurationMock.Setup(c => c["AppSettings:BaseUrl"]).Returns("https://localhost:7014");

            // Temporary HTML-template
            var templatesDir = Path.Combine(AppContext.BaseDirectory, "Templates");
            if (!Directory.Exists(templatesDir))
            {
                Directory.CreateDirectory(templatesDir);
            }
            File.WriteAllText(Path.Combine(templatesDir, "ConfRegistrationTemplate.html"), "Привіт, {Username}! Посилання: {confirmLink}");
        }

        [Fact]
        public async Task RegisterAsync_WhenDataIsValid_SuccessfullyRegistersUserAndSendsEmail()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var authService = new AuthService(context, _emailServiceMock.Object, _configurationMock.Object);

            var dto = new RegisterDto { Username = "vixy", Email = "vixy@test.com", Password = "SecurePassword123" };

            // Act
            var (success, error, field) = await authService.RegisterAsync(dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);
            Assert.Null(field);

            // checks if user is saved in database
            using var assertContext = new ApplicationDbContext(_options);
            var userInDb = await assertContext.Users.FirstOrDefaultAsync(u => u.Email == "vixy@test.com");

            Assert.NotNull(userInDb);
            Assert.Equal("vixy", userInDb.Username);

            // checks if password is hashed
            Assert.NotEqual("SecurePassword123", userInDb.PasswordHash);

           // checks token
            Assert.NotNull(userInDb.EmailConfirmationTokenHash);
            Assert.True(userInDb.EmailTokenExpiryDate > DateTime.UtcNow);

            // checks email sending with use of Email Mock
            _emailServiceMock.Verify(
                x => x.SendEmailAsync(
                    "vixy@test.com",
                    "Підтвердження реєстрації — Versum",
                    It.Is<string>(body => body.Contains("https://localhost:7014/api/Auth/confirm-email") && body.Contains("vixy"))
                ),
                Times.Once 
            );
        }

        [Fact]
        public async Task RegisterAsync_WhenUsernameAlreadyExists_ReturnsError()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            context.Users.Add(new User { Id = 1, Username = "existing_user", Email = "old@test.com", PasswordHash = "123yuyuyyuy" });
            await context.SaveChangesAsync();

            var authService = new AuthService(context, _emailServiceMock.Object, _configurationMock.Object);
            var dto = new RegisterDto { Username = "existing_user", Email = "new@test.com", Password = "Password123!" };

            // Act
            var (success, error, field) = await authService.RegisterAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Цей нікнейм вже існує", error);
            Assert.Equal("username", field);
        }

        [Fact]
        public async Task RegisterAsync_WhenEmailAlreadyExists_ReturnsError()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

 
            context.Users.Add(new User { Id = 1, Username = "old_user", Email = "existing@test.com", PasswordHash = "ere998rer" });
            await context.SaveChangesAsync();

            var authService = new AuthService(context, _emailServiceMock.Object, _configurationMock.Object);
            var dto = new RegisterDto { Username = "new_user", Email = "existing@test.com", Password = "Password123!" };

            // Act
            var (success, error, field) = await authService.RegisterAsync(dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Цей імейл вже існує", error);
            Assert.Equal("email", field);
        }
    }
}
