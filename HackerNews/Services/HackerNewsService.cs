using HackerNews.Interfaces;
using HackerNews.Mappers;
using HackerNews.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HackerNews.Services
{
    public class HackerNewsService : IHackerNewsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        
        private readonly ILogger<HackerNewsService> _logger;

        private readonly IMemoryCache _memoryCache;

        private const string _cacheKey = "BEST_STORIES";

        private double _cacheExpiry;

        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private CacheConfig _cacheConfig;

        public HackerNewsService(IHttpClientFactory httpClientFactory, ILogger<HackerNewsService> logger, IMemoryCache memoryCache, IOptions<CacheConfig> cacheConfig)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            
            _cacheConfig = cacheConfig.Value ?? throw new ArgumentNullException(nameof(cacheConfig));

            _cacheExpiry = _cacheConfig.CacheExpirySeconds;
        }

        public async Task<IEnumerable<Story>> GetBestStories(int? number, CancellationToken cancellationToken)
        {
            IEnumerable<Story>? orderedStories;

            if (!_memoryCache.TryGetValue(_cacheKey, out orderedStories))
            {
                await _semaphore.WaitAsync(cancellationToken);

                if (!_memoryCache.TryGetValue(_cacheKey, out orderedStories))
                {
                    try
                    {
                        var unorderedStories = await GetBestStories(cancellationToken).ConfigureAwait(false);

                        var options = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_cacheExpiry)
                        };

                        orderedStories = _memoryCache.Set(_cacheKey, unorderedStories == null ? null : unorderedStories.OrderByDescending(s => s.Score), options);
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }
            }

            return orderedStories == null ? Enumerable.Empty<Story>() : number.HasValue ? orderedStories.Take(number.Value) : orderedStories;
        }

        private async Task<IEnumerable<Story>> GetBestStories(CancellationToken cancellationToken)
        {
            var storyIds = await GetBestStoryIds(cancellationToken).ConfigureAwait(false);

            var stories = storyIds.Select(id =>
            {
                return GetBestStory(cancellationToken, id);
            });

            return await Task.WhenAll(stories);
        }

        private async Task<IEnumerable<int>> GetBestStoryIds(CancellationToken cancellationToken)
        {
            try
            {
                using (HttpClient client = _httpClientFactory.CreateClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync("beststories.json", cancellationToken).ConfigureAwait(false))
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                        {
                            return await JsonSerializer.DeserializeAsync<IEnumerable<int>>(stream, JsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false)
                                ?? throw new NullReferenceException("Story ids could not be deserialised from response.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve best story ids.");

                throw;
            }
        }

        private async Task<Story> GetBestStory(CancellationToken cancellationToken, int id)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            try
            {
                using (HttpClient client = _httpClientFactory.CreateClient())
                {
                    using (HttpResponseMessage response = await client.GetAsync($"item/{id}.json", cancellationToken).ConfigureAwait(false))
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                        {
                            var s = await JsonSerializer.DeserializeAsync<HackerStory>(stream, options, cancellationToken).ConfigureAwait(false)
                                ?? throw new NullReferenceException("Story could not be deserialised from response.");

                            return s.ToStory();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve best story for id : {0}.", id);

                throw;
            }
        }
    }
}
