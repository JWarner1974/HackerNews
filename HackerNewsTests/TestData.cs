using HackerNews.Mappers;
using HackerNews.Models;

namespace HackerNewsTests
{
    public class TestData
    {
        internal static HackerStory GetTestHackerStory()
        {
            return new HackerStory
            {
                By = "J Tester",
                Descendants = 31,
                Id = 8000,
                Kids = [8001, 8002],
                Score = 101,
                Time = 1175714200,
                Title = "This Is Not A Test",
                Type = "story",
                Url = "http=//www.mytests.com/mystory.html"
            };
        }

        internal static List<int> GetTestStoryIds()
        {
            var storyIds = new List<int>();

            for (var i = 0; i < 200; i++)
            {
                storyIds.Add(i);
            }

            return storyIds;
        }

        internal static List<Story> GetTestStories()
        {
            var stories = new List<Story>();

            for (var i = 0; i < 200; i++)
            {
                var story = GetTestHackerStory().ToStory();
                story.Title = $"Some title {i}";
                stories.Add(story);
            }

            return stories;
        }
    }
}
