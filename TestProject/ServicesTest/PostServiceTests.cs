using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Versum.Context;
using Versum.Dtos;
using Versum.Services;

namespace VersumTestProject.ServicesTest
{
    internal class PostServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationDbContext _assertContext;
        private readonly PostService _postService;

        // SET UP
        public PostServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;

            _context = new ApplicationDbContext(options);
            _postService = new PostService(_context);
            _assertContext = new ApplicationDbContext(options);
        }

             // TEARDOWN
        public void Dispose()
        {
            _context.Dispose();
            _assertContext.Dispose();

          
        }



        /*  [Fact]
          public async Task PublishDraftAsyncAsync_SuccessfullPublication()
          {
              // Arrange


              var dto = new PostDto { Title = TestUsername, Name = TestName, Bio = TestBio };

              // Act
              var (success, error) = await _postService.UpdateProfileAsync(99989897, dto);

              // Assert
              Assert.False(success);
              Assert.Equal("Чому нас вважають за одну людину?", error); 
          }


          */
    }
}

