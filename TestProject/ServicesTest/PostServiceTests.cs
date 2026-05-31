using Microsoft.EntityFrameworkCore;
using Moq;
using Versum;
using Versum.Context;
using Versum.Dtos;
using Versum.Models;
using Versum.Services;

namespace VersumTestProject.ServicesTest
{
    public class PostServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationDbContext _assertContext;
        private readonly PostService _postService;
        private readonly ProfileService _profileService;
        private readonly NotificationService _notifitationService;

        private const string TestTitle = "title";
        private const string TestDescription = "description";
        private const string TestContent = "SecurePassword123";
        private const string TestUsername = "vixy";
        private const string TestEmail = "vixy@test.com";
        private const string TestBio = "AuthorBio";

        // SET UP
        public PostServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            _context = new ApplicationDbContext(options);
            _postService = new PostService(_context, _profileService, _notifitationService);
            _assertContext = new ApplicationDbContext(options);
        }

        // TEARDOWN
        public void Dispose()
        {
            _context.Dispose();
            _assertContext.Dispose();


        }



        [Fact]
        public async Task CreateDraftAsync_ReturnSuccessWhenAthorFound()
        {
            // Arrange
            int Id = 1;
            var testuser = new User { Id = Id, Username = TestUsername, Email = TestEmail };
            var testauthor = new Author { AuthorId = Id, AuthorBio = TestBio };
            _context.Users.Add(testuser);
            _context.Authors.Add(testauthor);
            await _context.SaveChangesAsync();

            var dto = new CreateDraftDto { Title = TestTitle };

            // Act
            var (success, error, postId) = await _postService.CreateDraftAsync(testuser.Id, dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);
            Assert.NotNull(postId);

            var updatedAuthor = await _assertContext.Users.Include(u => u.AuthorProfile).FirstAsync(u => u.Id == 1);

            var postInDb = await _assertContext.Posts.FindAsync(postId);
            Assert.NotNull(postInDb);
            Assert.Equal(dto.Title, postInDb.Title);
            Assert.Equal(Id, postInDb.AuthorId);
            Assert.True(postInDb.IsDraft);

        }



        [Fact]
        public async Task CreateDraftAsync_ReturnErrorWhenAthorNOTFound()
        {
            // Arrange
            int Id = 1;
            var testuser = new User { Id = Id, Username = TestUsername, Email = TestEmail };
            _context.Users.Add(testuser);

            await _context.SaveChangesAsync();

            var dto = new CreateDraftDto { Title = TestTitle };

            // Act
            var (success, error, postId) = await _postService.CreateDraftAsync(testuser.Id, dto);

            // Assert
            Assert.False(success);
            Assert.Null(testuser.AuthorProfile);
            Assert.Equal("AuthorNotFound", error);
            Assert.Null(postId);


        }

        [Fact]
        public async Task CreateDraftAsync_ReturnsServerError_WhenDatabaseThrowsException()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;


            var mockContext = new Mock<ApplicationDbContext>(options) { CallBase = true };
            var testAuthor = new Author { AuthorId = 1, AuthorBio = TestBio };

            mockContext.Object.Authors.Add(testAuthor);
            await mockContext.Object.SaveChangesAsync();


            mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            var serviceWithMock = new PostService(mockContext.Object, _profileService, _notifitationService);
            var dto = new CreateDraftDto { Title = "Test Title" };

            // 5. Act
            var (success, error, postId) = await serviceWithMock.CreateDraftAsync(1, dto);

            // 6. Assert
            Assert.False(success);
            Assert.Equal("ServerError", error);
            Assert.Null(postId);
        }


        [Fact]
        public async Task UpdateDraftAsync_ReturnSuccess()
        {
            // Arrange
            int Id = 1;
            var testuser = new User { Id = Id, Username = TestUsername, Email = TestEmail };
            var testauthor = new Author { AuthorId = Id, AuthorBio = TestBio };
            var testdraft = new Post { Id = 1, Title = TestTitle, IsDraft = true, AuthorId = Id };
            _context.Users.Add(testuser);
            _context.Authors.Add(testauthor);
            _context.Posts.Add(testdraft);

            await _context.SaveChangesAsync();

            var dto = new PostDto { Title = TestTitle, Description = TestDescription, Content = TestContent};

            // Act
            var (success, error) = await _postService.UpdateDraftAsync(testdraft.Id,testuser.Id,dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);
         

            var updatedAuthor = await _assertContext.Users.Include(u => u.AuthorProfile).FirstAsync(u => u.Id == 1);

            var postInDb = await _assertContext.Posts.FindAsync(testdraft.Id);
            Assert.NotNull(postInDb);
            Assert.Equal(dto.Title, postInDb.Title);
            Assert.Equal(dto.Description, postInDb.Description);
            Assert.Equal(dto.Content, postInDb.Content);
           
        }
    

     [Fact]
        public async Task UpdateDraftAsync_ReturnFalseWnenDraftIsNOTFound()
        {
            // Arrange
            int Id = 1;
            var testuser = new User { Id = Id, Username = TestUsername, Email = TestEmail };
            var testauthor = new Author { AuthorId = Id, AuthorBio = TestBio };
            var testdraft = new Post { Id = 1, Title = TestTitle, Description = TestDescription, Content = TestContent, IsDraft = true};
            _context.Users.Add(testuser);
            _context.Authors.Add(testauthor);
            _context.Posts.Add(testdraft);

            await _context.SaveChangesAsync();

            var dto = new PostDto { Title = TestTitle, Description = TestDescription, Content = TestContent };

            // Act
            var (success, error) = await _postService.UpdateDraftAsync(9000, testuser.Id, dto);

            // Assert
            Assert.False(success);
            Assert.Equal("DraftNotFound", error);

        }

        [Fact]
        public async Task UpdateDraftAsync_ReturnFalseWnenDraftIsFALSE()
        {
            // Arrange
            int Id = 1;
            var testuser = new User { Id = Id, Username = TestUsername, Email = TestEmail };
            var testauthor = new Author { AuthorId = Id, AuthorBio = TestBio };
            var testdraft = new Post { Id = 1, Title = TestTitle, IsDraft = false };
            _context.Users.Add(testuser);
            _context.Authors.Add(testauthor);
            _context.Posts.Add(testdraft);

            await _context.SaveChangesAsync();

            var dto = new PostDto { Title = TestTitle, Description = TestDescription, Content = TestContent };

            // Act
            var (success, error) = await _postService.UpdateDraftAsync(testdraft.Id, testuser.Id, dto);

            // Assert
            Assert.False(success);
            Assert.Equal("You can't edit published writings", error);

        }
    

     [Fact]
        public async Task UpdateDraftAsync_ReturnFalseWnenUserIdNotEqualAuthorId()
        {
            // Arrange
            int Id = 1;
            var testuser = new User { Id = Id, Username = TestUsername, Email = TestEmail };
            var testauthor = new Author { AuthorId = 6, AuthorBio = TestBio };
            var testdraft = new Post { Id = 1, Title = TestTitle, IsDraft = true };
            _context.Users.Add(testuser);
            _context.Authors.Add(testauthor);
            _context.Posts.Add(testdraft);

            await _context.SaveChangesAsync();

            var dto = new PostDto { Title = TestTitle, Description = TestDescription, Content = TestContent };

            // Act
            var (success, error) = await _postService.UpdateDraftAsync(testdraft.Id, testuser.Id, dto);

            // Assert
            Assert.False(success);
            Assert.Equal("YouAreNotAnOwnerOfDraft", error);

        }

        [Fact]
        public async Task UpdateDraftAsync_ReturnsServerError_WhenDatabaseThrowsException()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;


            var mockContext = new Mock<ApplicationDbContext>(options) { CallBase = true };
            int Id = 1;
            var testuser = new User { Id = Id, Username = TestUsername, Email = TestEmail };
            var testauthor = new Author { AuthorId = Id, AuthorBio = TestBio };
            var testdraft = new Post { Id = 1, Title = TestTitle, IsDraft = true , AuthorId = Id};

            mockContext.Object.Users.Add(testuser);
            mockContext.Object.Authors.Add(testauthor);
            mockContext.Object.Posts.Add(testdraft);
            await mockContext.Object.SaveChangesAsync();


            mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database connection failed"));

            var serviceWithMock = new PostService(mockContext.Object, _profileService, _notifitationService);
            var dto = new PostDto { Title = TestTitle, Description = TestDescription, Content = TestContent };


            // 5. Act
            var (success, error) = await serviceWithMock.UpdateDraftAsync(testdraft.Id,testuser.Id, dto);

            // 6. Assert
            Assert.False(success);
            Assert.Equal("ServerError", error);
          
        }


        [Fact]
        public async Task UpdateDraftAsync_SuccessUpdateWithGenres()
        {
            // Arrange
            int Id = 1;
            var testuser = new User { Id = Id, Username = TestUsername, Email = TestEmail };
            var testauthor = new Author { AuthorId = Id, AuthorBio = TestBio };
            var testdraft = new Post { Id = 1, Title = TestTitle, IsDraft = true, AuthorId = Id };
            var sciFiGenre = new Genre { Name = "Sci-Fi" };
         
            _context.Users.Add(testuser);
            _context.Authors.Add(testauthor);
            _context.Posts.Add(testdraft);
            _context.Genres.Add(sciFiGenre);

            await _context.SaveChangesAsync();

            var dto = new PostDto { Title = TestTitle, Description = TestDescription, Content = TestContent, Genres = { "Sci-Fi" } };

            // Act
            var (success, error) = await _postService.UpdateDraftAsync(testdraft.Id, testuser.Id, dto);

            // Assert
            Assert.True(success);
            Assert.Null(error);

            var postInDb = await _assertContext.Posts
    .Include(p => p.Genres) 
    .FirstOrDefaultAsync(p => p.Id == testdraft.Id);

            Assert.NotNull(postInDb);
            Assert.Equal(dto.Title, postInDb.Title);
            Assert.Equal(dto.Description, postInDb.Description);
            Assert.Equal(dto.Content, postInDb.Content);


            Assert.Equal(dto.Genres.Count, postInDb.Genres.Count);
            foreach (var expectedGenre in dto.Genres)
            {
               
                Assert.Contains(postInDb.Genres, g => g.Name == expectedGenre);
            }
            var updatedAuthor = await _assertContext.Users.Include(u => u.AuthorProfile).FirstAsync(u => u.Id == 1);

        }



        [Fact]
        public async Task PublishDraftAsync_ReturnError_WhenContentIsEmpty()
        {
            // Arrange
            int userId = 1;
            var testpost = new Post
            {
                Id = 1,
                AuthorId = userId,
                Title = TestTitle,
                Description = TestDescription,
                Content = "", 
                IsDraft = true
            };

            _context.Posts.Add(testpost);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _postService.PublishDraftAsync(testpost.Id, userId);

            // Assert
            Assert.False(success);
            Assert.Equal("ContentRequired", error);
        }

        [Fact]
        public async Task PublishDraftAsync_ReturnError_WhendescriptionIsEmpty()
        {
            // Arrange
            int userId = 1;
            var testpost = new Post
            {
                Id = 1,
                AuthorId = userId,
                Title = TestTitle,
                Description = "",
                Content = TestContent,
                IsDraft = true
            };

            _context.Posts.Add(testpost);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _postService.PublishDraftAsync(testpost.Id, userId);

            // Assert
            Assert.False(success);
            Assert.Equal("DescriptionRequired", error);
        }

        [Fact]
        public async Task PublishDraftAsync_ReturnError_WhenTitleIsEmpty()
        {
            // Arrange
            int userId = 1;
            var testpost = new Post
            {
                Id = 1,
                AuthorId = userId,
                Title = " ",
                Description = TestDescription,
                Content = TestContent,
                IsDraft = true
            };

            _context.Posts.Add(testpost);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _postService.PublishDraftAsync(testpost.Id, userId);

            // Assert
            Assert.False(success);
            Assert.Equal("TitleRequired", error);
        }

        [Fact]
        public async Task PublishDraftAsync_ReturnError_WhenUserIsNotOwner()
        {
            // Arrange
            int ownerId = 1;
            int strangerId = 2;
            var testpost = new Post { Id = 1, AuthorId = ownerId, IsDraft = true, Title = TestTitle, Description = TestDescription, Content = TestContent };

            _context.Posts.Add(testpost);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _postService.PublishDraftAsync(testpost.Id, strangerId);

            // Assert
            Assert.False(success);
            Assert.Equal("YouAreNotAnOwnerOfDraft", error);
        }

        [Fact]
        public async Task PublishDraftAsync_ReturnError_WhenAlreadyPublished()
        {
            // Arrange
            int userId = 1;
            var testpost = new Post { Id = 1, AuthorId = userId, IsDraft = false, Title = TestTitle, Description = TestDescription, Content = TestContent };

            _context.Posts.Add(testpost);
            await _context.SaveChangesAsync();

            // Act
            var (success, error) = await _postService.PublishDraftAsync(testpost.Id, userId);

            // Assert
            Assert.False(success);
            Assert.Equal("AlreadyPublished", error);
        }




    }
}

