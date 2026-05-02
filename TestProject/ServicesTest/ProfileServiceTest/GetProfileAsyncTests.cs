using Microsoft.EntityFrameworkCore;
using Versum;
using Versum.Context; 
using Versum.Services;


namespace VersumTestProject.ServicesTest.ProfileServiceTest
{
    public class GetProfileAsyncTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public GetProfileAsyncTests()
        {
            // Setting up In-Memory database
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        }

        [Fact]
        public async Task GetProfileByUsernameAsync_WhenProfileExists_ReturnsUserProfileile()
        {
            //Arrange enviroment and data
            using var context = new ApplicationDbContext(_options);

            var testUser = new User { Id = 1, Username = "vixyyy7", Email = "test@example.com" };
            var testProfile = new UserProfile { Id = 1, UserId = 1, Name = "Viktoria", Bio = "67" };

            context.Users.Add(testUser);
            context.Profiles.Add(testProfile);
            await context.SaveChangesAsync();


            var profileService = new ProfileService(context);

            //Act: testing method
            var result = await profileService.GetProfileByUsernameAsync("vixyyy7");

            //Assert: checking result
            Assert.NotNull(result);
            Assert.Equal("67", result.Bio);
            Assert.Equal("Viktoria", result.Name);
            Assert.Equal("vixyyy7", result.Username);
        }

        [Fact]
        public async Task GetProfileByUsernameAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var profileService = new ProfileService(context);

            // Act
            var result = await profileService.GetProfileByUsernameAsync("nonexistent_user");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProfileByUsernameAsync_WhenUserExistsButProfileDoesNot_ReturnsNone()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            var testUser = new User { Id = 2, Username = "noprofile", Email = "no@example.com" };
            context.Users.Add(testUser);
            await context.SaveChangesAsync();

            var profileService = new ProfileService(context);

            // Act
            var result = await profileService.GetProfileByUsernameAsync("noprofile");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("none", result.Bio);
            Assert.Equal("none", result.Name);

        }

        [Fact]
        public async Task GetProfileByUsernameAsync_WhenUsernameIsNullOrEmpty_ThrowsArgumentException()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var profileService = new ProfileService(context);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await profileService.GetProfileByUsernameAsync("");
            });
        }
       
      
    }
}
