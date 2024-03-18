using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using HackerNews.Controllers;
using HackerNews.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsTests
{
    [TestFixture]
    public class StoryControllerTests
    {
        [Test]
        [TestCase(null, 1, typeof(OkObjectResult))]
        [TestCase(0, 1, typeof(OkObjectResult))]
        [TestCase(1, 1, typeof(OkObjectResult))]
        [TestCase(200, 1, typeof(OkObjectResult))]
        [TestCase(-1, 0, typeof(BadRequestObjectResult))]
        [TestCase(201, 0, typeof(BadRequestObjectResult))]
        public async Task When_stories_requested_then_response_is_returned(int? num, int serviceCalls, Type type)
        {
            var logger = GetMockLogger();
            var service = GetMockService();

            var controller = new StoryController(logger.Object, service.Object);

            var response = await controller.GetBestStoriesAsync(CancellationToken.None, num);

            Assert.That(response, Is.AssignableTo(type));

            service.Verify(x => x.GetBestStories(num, CancellationToken.None), Times.Exactly(serviceCalls));
        }

        private Mock<IHackerNewsService> GetMockService()
        {
            var mock = new Mock<IHackerNewsService>();

            mock.Setup(x => x.GetBestStories(It.IsAny<int?>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetTestStories());

            return mock;
        }

        private Mock<ILogger<StoryController>> GetMockLogger()
        {
            var mock = new Mock<ILogger<StoryController>>();

            return mock;
        }
    }
}
