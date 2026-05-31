using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Versum.Dtos;
using System.Security.Claims;
using Versum.Controllers;
using Versum.Services;

namespace VersumTestProject.ControllerTest
{
    public class PostControllerTests
    {
        private readonly Mock<IPostService> _postServiceMock;
        private readonly Mock<IProfileService> _profileServiceMock;
        private readonly PostsController _controller;

        private const string TestTitle = "title";
        private const string TestDescription = "description";
        private const string TestContent = "SecurePassword123";
        public PostControllerTests()
        {
            _postServiceMock = new Mock<IPostService>();
            _profileServiceMock = new Mock<IProfileService>();
            _controller = new PostsController( _postServiceMock.Object, _profileServiceMock.Object);

            // mocking authorized user
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
             {
                  new Claim(ClaimTypes.NameIdentifier, "1"),
                }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

      
        [Fact]
        public async Task PublishDraft_ReturnsUnauthorized_WhenClaimIsMissingOrInvalid()
        {
            // Arrange

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { }, "mock"));
            _controller.ControllerContext.HttpContext.User = user;

            // Act
            var result = await _controller.PublishDraft(1);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task PublishDraft_ReturnsNotFound_WhenAuthorProfileIsMissing()
        {
            // Arrange
            int postId = 1;
            int userId = 1;


            _postServiceMock
                .Setup(s => s.PublishDraftAsync(postId, userId))
                .ReturnsAsync((false, "AuthorNotFound"));

            // Act
            var result = await _controller.PublishDraft(postId);

            // Assert

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);


            Assert.Equal(404, notFoundResult.StatusCode);


            var response = notFoundResult.Value;
            var message = response.GetType().GetProperty("message")?.GetValue(response, null);

            Assert.Equal("Профіль автора не знайдено", message);
        }

        [Fact]
        public async Task PublishDraft_ReturnsBadRequest_WhenServiceReturnsGeneralError()
        {
            // Arrange
            int postId = 1;
            int userId = 1;
            string specificError = "PostIsAlreadyPublished"; 

            _postServiceMock
                .Setup(s => s.PublishDraftAsync(postId, userId))
                .ReturnsAsync((false, specificError));

            // Act
            var result = await _controller.PublishDraft(postId);

            // Assert
           
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

          
            var response = badRequestResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);

           
            Assert.Equal(specificError, message);
        }

        [Fact]
        public async Task PublishDraft_ReturnsCreated_WhenPublishingIsSuccessful()
        {
            // Arrange
            int postId = 1;
            int userId = 1;

           
            _postServiceMock
                .Setup(s => s.PublishDraftAsync(postId, userId))
                .ReturnsAsync((true, string.Empty));

            // Act
            var result = await _controller.PublishDraft(postId);

            // Assert
            
            var objectResult = Assert.IsType<ObjectResult>(result);

           
            Assert.Equal(201, objectResult.StatusCode);

          
            var response = objectResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);

            Assert.Equal("Твір успішно опубліковано", message);
        }


        [Fact]
        public async Task CreateDraft_ReturnsUnauthorized_WhenClaimIsMissingOrInvalid()
        {
            // Arrange

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { }, "mock"));
            _controller.ControllerContext.HttpContext.User = user;
            var dto = new CreateDraftDto { };
            // Act
            var result = await _controller.CreateDraft(dto);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task CreateDraft_ReturnsNotFound_WhenAuthorProfileIsMissing()
        {
            // Arrange

            int userId = 1;
            var dto = new CreateDraftDto { Title = TestTitle};

            _postServiceMock
                .Setup(s => s.CreateDraftAsync(userId,dto))
                .ReturnsAsync((false, "AuthorNotFound",0));

            // Act
            var result = await _controller.CreateDraft(dto);

            // Assert

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);


            Assert.Equal(404, notFoundResult.StatusCode);


            var response = notFoundResult.Value;
            var message = response.GetType().GetProperty("message")?.GetValue(response, null);

            Assert.Equal("Профіль автора не знайдено", message);
        }

        [Fact]
        public async Task CreateDraft_ReturnsBadRequest_WhenServiceReturnsError()
        {
            // Arrange
            var dto = new CreateDraftDto { Title = TestTitle};
            int authorId = 1;
            string serviceError = "LimitExceeded";

            _postServiceMock
                .Setup(s => s.CreateDraftAsync(authorId, dto))
                .ReturnsAsync((false, serviceError, 0));

            // Act
            var result = await _controller.CreateDraft(dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var response = badRequestResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);
            Assert.Equal(serviceError, message);
        }

        [Fact]
        public async Task CreateDraft_WhenCreateIsSuccessfu()
        {
            // Arrange
            var dto = new CreateDraftDto { Title = "Нова чернетка"};
            int authorId = 1;
            int expostId = 42;

            _postServiceMock
                .Setup(s => s.CreateDraftAsync(authorId, dto))
                .ReturnsAsync((true, string.Empty, expostId));

            // Act
            var result = await _controller.CreateDraft(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);

          
            var response = objectResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);
            var postId = response?.GetType().GetProperty("postId")?.GetValue(response, null);

            Assert.Equal("Чернетку створено", message);
            Assert.Equal(expostId, postId);
        }

        [Fact]
        public async Task UpdateDraft_ReturnsUnauthorized_WhenClaimIsMissingOrInvalid()
        {
            // Arrange

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { }, "mock"));
            _controller.ControllerContext.HttpContext.User = user;
            var dto = new PostDto { };

            // Act

            var result = await _controller.UpdateDraft(1,dto);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateDraft_ReturnsNotFound_WhenDraftIsMisssing()
        {
            // Arrange

            int userId = 1;
            int postId = 1;
            var dto = new PostDto { Title = TestTitle, Description = TestDescription ,Content = TestContent };

            _postServiceMock
                .Setup(s => s.UpdateDraftAsync(postId,userId, dto))
                .ReturnsAsync((false, "DraftNotFound"));

            // Act
            var result = await _controller.UpdateDraft(1,dto);

            // Assert

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);


            Assert.Equal(404, notFoundResult.StatusCode);


            var response = notFoundResult.Value;
            var message = response.GetType().GetProperty("message")?.GetValue(response, null);

            Assert.Equal("Чернетку не знайдено", message);
        }

        [Fact]
        public async Task UpdateDraft_ReturnsNotFound_WhenDraftIsPublished()
        {
            // Arrange

            int userId = 1;
            int postId = 1;
            var dto = new PostDto { Title = TestTitle, Description = TestDescription, Content = TestContent };

            _postServiceMock
                .Setup(s => s.UpdateDraftAsync(postId, userId, dto))
                .ReturnsAsync((false, "You can't edit published writings"));

            // Act
            var result = await _controller.UpdateDraft(1, dto);

            // Assert

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);


            Assert.Equal(409, conflictResult.StatusCode);


            var response = conflictResult.Value;
            var message = response.GetType().GetProperty("message")?.GetValue(response, null);

            Assert.Equal("Твір уже опубліковано", message);
        }

        [Fact]
        public async Task UpdateDraft_ReturnsNotFound_WhenWrongAuthor()
        {
            // Arrange

            int userId = 1;
            int postId = 1;
            var dto = new PostDto { Title = TestTitle, Description = TestDescription, Content = TestContent };

            _postServiceMock
                .Setup(s => s.UpdateDraftAsync(postId, userId, dto))
                .ReturnsAsync((false, "YouAreNotAnOwnerOfDraft"));

            // Act
            var result = await _controller.UpdateDraft(1, dto);

            // Assert

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);


            Assert.Equal(404, notFoundResult.StatusCode);


            var response = notFoundResult.Value;
            var message = response.GetType().GetProperty("message")?.GetValue(response, null);

            Assert.Equal("Ви нє автором чернетки", message);
        }

        [Fact]
        public async Task UpdateDraft_ReturnsBadRequest_WhenServiceReturnsError()
        {
            // Arrange

            int userId = 1;
            int postId = 1;
            var dto = new PostDto { Title = TestTitle, Description = TestDescription, Content = TestContent };
        
            string serviceError = "ContentRequired";

            _postServiceMock
                .Setup(s => s.UpdateDraftAsync(postId,userId, dto))
                .ReturnsAsync((false, serviceError));

            // Act
            var result = await _controller.UpdateDraft(postId,dto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);

            var response = badRequestResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);
            Assert.Equal(serviceError, message);
        }

        [Fact]
        public async Task UpdateDraft_WhenUpdateIsSuccessfu()
        {
            // Arrange
            int userId = 1;
            int postId = 1;
            var dto = new PostDto { Title = TestTitle, Description = TestDescription, Content = TestContent };

            _postServiceMock
                .Setup(s => s.UpdateDraftAsync(postId,userId, dto))
                .ReturnsAsync((true, string.Empty));

            // Act
            var result = await _controller.UpdateDraft(postId,dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, objectResult.StatusCode);


            var response = objectResult.Value;
            var message = response?.GetType().GetProperty("message")?.GetValue(response, null);
         

            Assert.Equal("Чернетку збережено", message);
            
        }
    }
}
