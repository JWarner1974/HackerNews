using Microsoft.Extensions.Logging;
using HackerNews.Services;
using Moq;
using NUnit.Framework;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using HackerNews.Models;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;

namespace HackerNewsTests
{
    [TestFixture]
    public class HackerStoryServiceTests
    {
        private delegate void CacheCallback(object key, out object stories);

        [Test]
        [TestCase(true, 0, 1)]
        [TestCase(false, 200 + 1, 2)]
        public async Task When_n_stories_requested_n_are_returned(bool useCache, int httpCalls, int cacheCalls)
        {
            var httpClientFactoryMock = GetMockHttpClientFactory();
            var loggerMock = GetMockLogger();
            var cacheMock = GetMockMemoryCache(useCache);
            var configMock = GetMockCacheConfig();
            var num = 15;

            var service = new HackerNewsService(httpClientFactoryMock.Object, loggerMock.Object, cacheMock.Object, configMock.Object);

            var stories = await service.GetBestStories(num, CancellationToken.None);

            Assert.That(stories, Is.Not.Null);
            Assert.That(stories.Count(), Is.EqualTo(num));

            cacheMock.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object?>.IsAny), Times.Exactly(cacheCalls));
            httpClientFactoryMock.Verify(x => x.CreateClient(Options.DefaultName), Times.Exactly(httpCalls));
        }

        [Test]
        public async Task When_stories_returned_they_are_sorted_by_score()
        {
            var httpClientFactoryMock = GetMockHttpClientFactory();
            var loggerMock = GetMockLogger();
            var cacheMock = GetMockMemoryCache(true);
            var configMock = GetMockCacheConfig();
            var num = 10;

            var service = new HackerNewsService(httpClientFactoryMock.Object, loggerMock.Object, cacheMock.Object, configMock.Object);

            var stories = (await service.GetBestStories(num, CancellationToken.None)).ToList();

            for (var i = 0; i < num - 1; i++)
            {
                Assert.That(stories[i].Score <= stories[i + 1].Score);
            }
        }

        private Mock<HttpMessageHandler> GetMockMessageHandler()
        {
            string storyContent = JsonConvert.SerializeObject(TestData.GetTestStories().First());
            string storyIdContent = JsonConvert.SerializeObject(TestData.GetTestStoryIds());

            HttpResponseMessage response = new HttpResponseMessage();

            var messageHandler = new Mock<HttpMessageHandler>();
            messageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Callback<HttpRequestMessage, CancellationToken>((req, ct) =>
                {
                    if (req.RequestUri.AbsolutePath.Contains("item"))
                    {
                        response = new HttpResponseMessage
                        {
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Content = new StringContent(storyContent),
                        };
                    }
                    else
                    {
                        response = new HttpResponseMessage
                        {
                            StatusCode = System.Net.HttpStatusCode.OK,
                            Content = new StringContent(storyIdContent),
                        };
                    }
                })
                .ReturnsAsync(() =>
                {
                    return response;
                })
                .Verifiable();

            return messageHandler;
        }

        private Mock<IHttpClientFactory> GetMockHttpClientFactory()
        {
            var mockFactory = new Mock<IHttpClientFactory>();
            var mockMessageHandler = GetMockMessageHandler();

            mockFactory.Setup(x => x.CreateClient(Options.DefaultName)).Returns(
                () =>
                {
                    return new HttpClient(mockMessageHandler.Object)
                    {
                        BaseAddress = new Uri("http://sometestdomain")
                    };
                }
            );

            return mockFactory;
        }

        private Mock<ILogger<HackerNewsService>> GetMockLogger()
        {
            var mock = new Mock<ILogger<HackerNewsService>>();

            return mock;
        }

        private Mock<IMemoryCache> GetMockMemoryCache(bool cacheContainsItem)
        {
            var mock = new Mock<IMemoryCache>();
            var mockEntry = new Mock<ICacheEntry>();

            object? stories = cacheContainsItem ? TestData.GetTestStories() : null;

            mockEntry.SetupSet(x => x.Value = It.IsAny<object?>());
            mockEntry.SetupSet(x => x.Value = It.IsAny<MemoryCacheEntryOptions?>());

            mock.Setup(x => x.TryGetValue(It.IsAny<object>(), out stories))
                .Callback(new CacheCallback(
                    (object key, out object outputStories) =>
                    {
                        outputStories = stories;
                    }))
                .Returns(cacheContainsItem);

            mock.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(mockEntry.Object);

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
