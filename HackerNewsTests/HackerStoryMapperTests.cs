using HackerNews.Mappers;
using HackerNews.Models;
using NUnit.Framework;

namespace HackerNewsTests
{
    [TestFixture]
    public class HackerStoryMapperTests
    {
        [Test]
        public void When_hacker_story_is_converted_then_story_is_populated()
        {
            var hackerStory = TestData.GetTestHackerStory();

            var story = hackerStory.ToStory();

            Assert.That(story, Is.Not.Null);
            Assert.That(story.Title, Is.EqualTo(hackerStory.Title));
            Assert.That(story.Uri, Is.EqualTo(hackerStory.Url));
            Assert.That(story.PostedBy, Is.EqualTo(hackerStory.By));
            Assert.That(story.Time, Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(hackerStory.Time)));
            Assert.That(story.Score, Is.EqualTo(hackerStory.Score));
            Assert.That(story.CommentCount, Is.EqualTo(hackerStory.Kids == null ? 0 : hackerStory.Kids.Length));
        }

        [Test]
        public void When_hacker_story_is_null_then_throws()
        {
            HackerStory hackerStory = null;

            Assert.Throws(typeof(ArgumentNullException), () => hackerStory.ToStory());
        }
    }
}
