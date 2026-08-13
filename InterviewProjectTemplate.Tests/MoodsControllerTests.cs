using InterviewProjectTemplate;
using InterviewProjectTemplate.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace InterviewProjectTemplate.Tests
{
    public class MoodsControllerTests
    {
        private static MoodTrackerDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<MoodTrackerDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new MoodTrackerDbContext(options);
        }

        private static IConfiguration CreateConfiguration(string adminKey = "test-key")
        {
            var settings = new Dictionary<string, string?>
            {
                { "AdminApiKey", adminKey }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public async Task SubmitMood_FirstSubmissionOfDay_Succeeds()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var controller = new MoodsController(dbContext, CreateConfiguration())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
            var request = new SubmitMoodRequest { Rating = MoodRating.PrettyGood, Comment = "Good day" };

            // Act
            var result = await controller.SubmitMood(request);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Single(dbContext.MoodEntries);
        }

        [Fact]
        public async Task SubmitMood_SecondSubmissionSameDay_ReturnsBadRequest()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var firstHttpContext = new DefaultHttpContext();
            var controller = new MoodsController(dbContext, CreateConfiguration())
            {
                ControllerContext = new ControllerContext { HttpContext = firstHttpContext }
            };
            var request = new SubmitMoodRequest { Rating = MoodRating.Meh };

            // Act: first submission
            await controller.SubmitMood(request);

            // Simulate the browser: extract the Set-Cookie header from the first response,
            // and manually attach it as the Cookie header on a second request context,
            // so the second call "remembers" who this user is, just like a real browser would.
            var setCookieHeader = firstHttpContext.Response.Headers["Set-Cookie"].ToString();
            var cookieValue = setCookieHeader.Split(';')[0]; // e.g. "moodTrackerUserId=abc123"

            var secondHttpContext = new DefaultHttpContext();
            secondHttpContext.Request.Headers["Cookie"] = cookieValue;
            controller.ControllerContext = new ControllerContext { HttpContext = secondHttpContext };

            // Act: second submission, same simulated browser
            var secondResult = await controller.SubmitMood(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(secondResult);
            Assert.Single(dbContext.MoodEntries); // still only 1 row, second one was rejected
        }

        [Fact]
        public async Task GetAllMoods_WithoutAdminKey_ReturnsUnauthorized()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            var controller = new MoodsController(dbContext, CreateConfiguration());

            // Act
            var result = await controller.GetAllMoods(adminKey: null);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetAllMoods_WithCorrectAdminKey_ReturnsAllEntriesMostRecentFirst()
        {
            // Arrange
            var dbContext = CreateInMemoryDbContext();
            dbContext.MoodEntries.Add(new MoodEntry
            {
                Id = Guid.NewGuid(),
                UserIdentifier = "user1",
                Rating = MoodRating.Meh,
                CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
            });
            dbContext.MoodEntries.Add(new MoodEntry
            {
                Id = Guid.NewGuid(),
                UserIdentifier = "user2",
                Rating = MoodRating.FeelingGreat,
                CreatedAtUtc = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();

            var controller = new MoodsController(dbContext, CreateConfiguration("test-key"));

            // Act
            var result = await controller.GetAllMoods(adminKey: "test-key");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var entries = Assert.IsAssignableFrom<List<MoodEntry>>(okResult.Value);
            Assert.Equal(2, entries.Count);
            Assert.Equal(MoodRating.FeelingGreat, entries[0].Rating); // most recent first
        }
    }
}