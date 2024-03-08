using HackerNews.Interfaces;
using HackerNews.Models;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StoryController : ControllerBase
    {
        private readonly ILogger<StoryController> _logger;
        private readonly IHackerNewsService _hackerNewsService;

        public StoryController(ILogger<StoryController> logger, IHackerNewsService hackerNewsService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hackerNewsService = hackerNewsService ?? throw new ArgumentNullException(nameof(hackerNewsService));
        }

        [HttpGet]
        [Route("[action]/{number?}")]
        public async Task<ActionResult<IEnumerable<Story>>> GetBestStoriesAsync(CancellationToken cancellationToken, int? number = null)
        {
            try
            {
                if (number.HasValue)
                {
                    if (number.Value < 0 || number.Value > 200)
                    {
                        return BadRequest("Invalid number supplied - must be between 0 and 200.");
                    }
                }

                var stories = await _hackerNewsService.GetBestStories(number, cancellationToken);

                return Ok(stories);
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Critical, ex.Message);

                return BadRequest("Failed to retrieve best stories.");
            }
        }
    }
}
