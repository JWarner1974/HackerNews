using HackerNews.Models;

namespace HackerNews.Interfaces
{
    public interface IHackerNewsService
    {
        Task<IEnumerable<Story>> GetBestStories(int? number, CancellationToken cancellationToken);
    }
}
