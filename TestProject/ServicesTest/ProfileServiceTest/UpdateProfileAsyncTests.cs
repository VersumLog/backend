using Microsoft.EntityFrameworkCore;
using Versum;
using Versum.Context;
using Versum.Dtos;
using Versum.Services;
using Xunit;

namespace VersumTestProject.ServicesTest.ProfileServiceTest
{
    public class UpdateProfileAsyncTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public UpdateProfileAsyncTests()
        {
            
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        }


        [Fact]
        public async Task UpdateProfileAsync_WhenUserExists_UpdatesDataAndReturnsSuccess()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            var testUser = new User { Id = 1, Username = "old_username", Email = "test@example.com" };
            var testProfile = new UserProfile { Id = 1, UserId = 1, Name = "Old Name", Bio = "Old Bio" };

            context.Users.Add(testUser);
            context.Profiles.Add(testProfile);
            await context.SaveChangesAsync();

            var profileService = new ProfileService(context);
            var dto = new UserProfileDto { Username = "new_username", Name = "New Name", Bio = "New Bio" };

            // Act
            var (success, error) = await profileService.UpdateProfileAsync(1, dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);

            using var assertContext = new ApplicationDbContext(_options);
            var updatedUser = await assertContext.Users.Include(u => u.Profile).FirstAsync(u => u.Id == 1);

            Assert.Equal("new_username", updatedUser.Username);
            Assert.Equal("New Name", updatedUser.Profile.Name);
            Assert.Equal("New Bio", updatedUser.Profile.Bio);
        }


        [Fact]
        public async Task UpdateProfileAsync_WhenUserDoesNotExist_ReturnsFalseAndErrorMessage()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);
            var profileService = new ProfileService(context);
            var dto = new UserProfileDto { Username = "vixy", Name = "Viktoria", Bio = "Bio" };

            // Act
            var (success, error) = await profileService.UpdateProfileAsync(99989897, dto); 

            // Assert
            Assert.False(success);
            Assert.Equal("Чому нас вважають за одну людину?", error);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenUsernameIsTakenByAnotherUser_ReturnsFalseAndErrorMessage()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

          
            var userToUpdate = new User { Id = 1, Username = "user1", Email = "u1@test.com" };
            var existingUser = new User { Id = 2, Username = "taken_username", Email = "u2@test.com" };

            context.Users.AddRange(userToUpdate, existingUser);
            await context.SaveChangesAsync();

            var profileService = new ProfileService(context);
            var dto = new UserProfileDto { Username = "taken_username", Name = "Any Name", Bio = "Any Bio" };

            // Act
            var (success, error) = await profileService.UpdateProfileAsync(1, dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Цей нікнейм вже існує", error);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenUserHasNoProfileYet_CreatesNewProfileAndReturnsSuccess()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            
            var userWithoutProfile = new User { Id = 1, Username = "vixy", Email = "test@test.com" };
            context.Users.Add(userWithoutProfile);
            await context.SaveChangesAsync();

            var profileService = new ProfileService(context);
            var dto = new UserProfileDto { Username = "vixy", Name = "Viktoria", Bio = "Bio" };

            // Act
            var (success, error) = await profileService.UpdateProfileAsync(1, dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);

           
            using var assertContext = new ApplicationDbContext(_options);
            var updatedUser = await assertContext.Users.Include(u => u.Profile).FirstAsync(u => u.Id == 1);

            Assert.NotNull(updatedUser.Profile);
            Assert.Equal("Viktoria", updatedUser.Profile.Name);
            Assert.Equal("Bio", updatedUser.Profile.Bio);
        }

        [Fact]
        public async Task UpdateBio_WhenKeepingOwnUsername_ReturnsSuccess()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            var user = new User { Id = 1, Username = "vixy", Email = "test@test.com" };
            var profile = new UserProfile { Id = 1, UserId = 1, Name = "Viktoria", Bio = "Old Bio" };

            context.Users.Add(user);
            context.Profiles.Add(profile);
            await context.SaveChangesAsync();

            var profileService = new ProfileService(context);

            
            var dto = new UserProfileDto { Username = "vixy", Name = "Viktoria", Bio = "New Bio" };

            // Act
            var (success, error) = await profileService.UpdateProfileAsync(1, dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);

           
            using var assertContext = new ApplicationDbContext(_options);
            var updatedProfile = await assertContext.Profiles.FirstAsync(p => p.UserId == 1);
            Assert.Equal("New Bio", updatedProfile.Bio);
        }
        [Fact]
        public async Task UpdateName_WhenKeepingOwnUsername_ReturnsSuccess()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            var user = new User { Id = 1, Username = "vixy", Email = "test@test.com" };
            var profile = new UserProfile { Id = 1, UserId = 1, Name = "Viktoria", Bio = "Old Bio" };

            context.Users.Add(user);
            context.Profiles.Add(profile);
            await context.SaveChangesAsync();

            var profileService = new ProfileService(context);


            var dto = new UserProfileDto { Username = "vixy", Name = "Vik", Bio = "Old Bio" };

            // Act
            var (success, error) = await profileService.UpdateProfileAsync(1, dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);


            using var assertContext = new ApplicationDbContext(_options);
            var updatedProfile = await assertContext.Profiles.FirstAsync(p => p.UserId == 1);
            Assert.Equal("Vik", updatedProfile.Name);
        }

        [Fact]
        public async Task UpdateUserNameOnlyReturnsSuccess()
        {
            // Arrange
            using var context = new ApplicationDbContext(_options);

            var user = new User { Id = 1, Username = "vixy", Email = "test@test.com" };
            var profile = new UserProfile { Id = 1, UserId = 1, Name = "Viktoria", Bio = "Old Bio" };

            context.Users.Add(user);
            context.Profiles.Add(profile);
            await context.SaveChangesAsync();

            var profileService = new ProfileService(context);


            var dto = new UserProfileDto { Username = "vixy7788", Name = "Viktoria", Bio = "Old Bio" };

            // Act
            var (success, error) = await profileService.UpdateProfileAsync(1, dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);


            using var assertContext = new ApplicationDbContext(_options);
            var updatedUser = await assertContext.Users.Include(u => u.Profile).FirstAsync(u => u.Id == 1);
            Assert.Equal("vixy7788", updatedUser.Username);
        }

    }
}
