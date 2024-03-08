using Microsoft.Extensions.Logging;
using HackerNews.Services;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using HackerNews.Models;

namespace HackerNewsTests
{
    [TestFixture]
    public class HackerStoryServiceTests
    {
        private delegate void CacheCallback(object key, out object stories);

        [Test]
        public async Task When_n_stories_requested_n_are_returned()
        {
            var httpClientFactoryMock = GetMockHttpClientFactory();
            var loggerMock = GetMockLogger();
            var cacheMock = GetMockMemoryCache();
            var configMock = GetMockCacheConfig();
            var num = 15;

            var service = new HackerNewsService(httpClientFactoryMock.Object, loggerMock.Object, cacheMock.Object, configMock.Object);

            var stories = await service.GetBestStories(num, CancellationToken.None);

            Assert.That(stories, Is.Not.Null);
            Assert.That(stories.Count(), Is.EqualTo(num));
        }

        private Mock<IHttpClientFactory> GetMockHttpClientFactory()
        {
            var mock = new Mock<IHttpClientFactory>();

            return mock;
        }

        private Mock<ILogger<HackerNewsService>> GetMockLogger()
        {
            var mock = new Mock<ILogger<HackerNewsService>>();

            return mock;
        }

        private Mock<IMemoryCache> GetMockMemoryCache()
        {
            var mock = new Mock<IMemoryCache>();

            object stories = TestData.GetTestStories();

            mock.Setup(x => x.TryGetValue(It.IsAny<string>(), out stories))
                .Callback(new CacheCallback(
                    (object key, out object outputStories) =>
                    {
                        outputStories = stories;
                    }))
                .Returns(true);

            mock.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<IOrderedEnumerable<Story>>(), It.IsAny<MemoryCacheEntryOptions?>())).Returns(stories as IOrderedEnumerable<Story>);

            return mock;
        }

        private Mock<IOptions<CacheConfig>> GetMockCacheConfig()
        {
            var mock = new Mock<IOptions<CacheConfig>>();

            mock.SetupGet(x => x.Value).Returns(new CacheConfig
            {
                CacheExpirySeconds = 3600,
            });

            return mock;
        }
    }
}
