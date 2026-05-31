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
    public class ProfileServiceTests : IDisposable
    {

        private readonly ApplicationDbContext _context;
        private readonly ApplicationDbContext _assertContext;
        private readonly ProfileService _profileService;
        private readonly NotificationService _notificationsService;

        private const string TestUsername = "vixy";
        private const string UpdatedUsername = "vixy_upd";
        private const string TestEmail = "vixy@test.com";
        private const string TestBio = "bio";
        private const string TestName = "viktoria";


        // SET UP
        public ProfileServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            _context = new ApplicationDbContext(options);
            _profileService = new ProfileService(_context, _notificationsService);
            _assertContext = new ApplicationDbContext(options);
        }


        // TEARDOWN
        public void Dispose()
        {
            _context.Dispose();
            _assertContext.Dispose();

        }

        [Fact]
        public async Task GetProfileByUsernameAsync_WhenProfileExists_ReturnsUserProfileile()
        {
            //Arrange data

            var testUser = new User { Id = 1, Username = TestUsername, Email = TestEmail };
            var testProfile = new UserProfile { Id = 1, UserId = 1, Name = TestName, Bio = TestBio };

            _context.Users.Add(testUser);
            _context.Profiles.Add(testProfile);
            await _context.SaveChangesAsync();

            //Act: testing method
            var result = await _profileService.GetProfileByUsernameAsync(TestUsername, 1);

            //Assert: checking result
            Assert.NotNull(result);
            Assert.Equal(TestBio, result.Bio);
            Assert.Equal(TestName, result.Name);
            Assert.Equal(TestUsername, result.Username);
        }

        [Fact]
        public async Task GetProfileByUsernameAsync_WhenUserDoesNotExist_ReturnsNull()
        {
            // Arrange

            // Act
            var result = await _profileService.GetProfileByUsernameAsync("nonexistent_user", 1);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetProfileByUsernameAsync_WhenUserExistsButProfileDoesNot_ReturnsNone()
        {
            // Arrange

            var testUser = new User { Id = 2, Username = "noprofile", Email = "no@example.com" };
            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            // Act
            var result = await _profileService.GetProfileByUsernameAsync("noprofile", 2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("none", result.Bio);
            Assert.Equal("none", result.Name);

        }

        [Fact]
        public async Task GetProfileByUsernameAsync_WhenUsernameIsNullOrEmpty_ThrowsArgumentException()
        {
            // Arrange
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _profileService.GetProfileByUsernameAsync("", 2);
            });
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenUserExists_UpdatesDataAndReturnsSuccess()
        {
            // Arrange

            var testUser = new User { Id = 1, Username = "old_username", Email = TestEmail };
            var testProfile = new UserProfile { Id = 1, UserId = 1, Name = "Old Name", Bio = "Old Bio" };

            _context.Users.Add(testUser);
            _context.Profiles.Add(testProfile);
            await _context.SaveChangesAsync();

            var dto = new UserProfileDto { Username = "new_username", Name = "New Name", Bio = "New Bio" };

            // Act
            var (success, error) = await _profileService.UpdateProfileAsync(1, dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);


            var updatedUser = await _assertContext.Users.Include(u => u.Profile).FirstAsync(u => u.Id == 1);

            Assert.Equal("new_username", updatedUser.Username);
            Assert.Equal("New Name", updatedUser.Profile.Name);
            Assert.Equal("New Bio", updatedUser.Profile.Bio);
        }


        [Fact]
        public async Task UpdateProfileAsync_WhenUserDoesNotExist_ReturnsFalseAndErrorMessage()
        {
            // Arrange


            var dto = new UserProfileDto { Username = TestUsername, Name = TestName, Bio = TestBio };

            // Act
            var (success, error) = await _profileService.UpdateProfileAsync(99989897, dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Чому нас вважають однією людиною?", error);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenUsernameIsTakenByAnotherUser_ReturnsFalseAndErrorMessage()
        {
            // Arrange
            var userToUpdate = new User { Id = 1, Username = "user1", Email = "u1@test.com" };
            var existingUser = new User { Id = 2, Username = "taken_username", Email = "u2@test.com" };

            _context.Users.AddRange(userToUpdate, existingUser);
            await _context.SaveChangesAsync();


            var dto = new UserProfileDto { Username = "taken_username", Name = "Any Name", Bio = "Any Bio" };

            // Act
            var (success, error) = await _profileService.UpdateProfileAsync(1, dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Цей нікнейм вже існує", error);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenUserHasNoProfileYet_CreatesNewProfileAndReturnsSuccess()
        {
            // Arrange

            var userWithoutProfile = new User { Id = 1, Username = TestUsername, Email = TestEmail };
            _context.Users.Add(userWithoutProfile);
            await _context.SaveChangesAsync();


            var dto = new UserProfileDto { Username = TestUsername, Name = TestName, Bio = TestBio };

            // Act
            var (success, error) = await _profileService.UpdateProfileAsync(1, dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);


            var updatedUser = await _assertContext.Users.Include(u => u.Profile).FirstAsync(u => u.Id == 1);

            Assert.NotNull(updatedUser.Profile);
            Assert.Equal(TestName, updatedUser.Profile.Name);
            Assert.Equal(TestBio, updatedUser.Profile.Bio);
        }

        [Fact]
        public async Task UpdateBio_WhenKeepingOwnUsername_ReturnsSuccess()
        {
            // Arrange
            var user = new User { Id = 1, Username = TestUsername, Email = TestEmail };
            var profile = new UserProfile { Id = 1, UserId = 1, Name = TestName, Bio = "Old Bio" };

            _context.Users.Add(user);
            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();


            var dto = new UserProfileDto { Username = TestUsername, Name = "new_name", Bio = "New Bio" };

            // Act
            var (success, error) = await _profileService.UpdateProfileAsync(1, dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);

            var updatedProfile = await _assertContext.Profiles.FirstAsync(p => p.UserId == 1);
            Assert.Equal("New Bio", updatedProfile.Bio);
        }
        [Fact]
        public async Task UpdateName_WhenKeepingOwnUsername_ReturnsSuccess()
        {
            // Arrange

            var user = new User { Id = 1, Username = TestUsername, Email = TestEmail };
            var profile = new UserProfile { Id = 1, UserId = 1, Name = TestName, Bio = "Old Bio" };

            _context.Users.Add(user);
            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();

            var dto = new UserProfileDto { Username = TestUsername, Name = "Vik", Bio = "Old Bio" };

            // Act
            var (success, error) = await _profileService.UpdateProfileAsync(1, dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);

            var updatedProfile = await _assertContext.Profiles.FirstAsync(p => p.UserId == 1);
            Assert.Equal("Vik", updatedProfile.Name);
        }

        [Fact]
        public async Task UpdateUserNameOnlyReturnsSuccess()
        {
            // Arrange

            var user = new User { Id = 1, Username = TestUsername, Email = TestEmail };
            var profile = new UserProfile { Id = 1, UserId = 1, Name = TestName, Bio = "Old Bio" };

            _context.Users.Add(user);
            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();

            var dto = new UserProfileDto { Username = UpdatedUsername, Name = TestName, Bio = "Old Bio" };

            // Act
            var (success, error) = await _profileService.UpdateProfileAsync(1, dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);


            var updatedUser = await _assertContext.Users.Include(u => u.Profile).FirstAsync(u => u.Id == 1);
            Assert.Equal(UpdatedUsername, updatedUser.Username);
        }

    }
}

