using HackerNews.Models;

namespace HackerNews.Mappers
{
    public static class HackerStoryMapper
    {
        public static Story ToStory(this HackerStory hackerStory)
        {
            if (hackerStory == null)
            {
                throw new ArgumentNullException(nameof(hackerStory));
            }

            if (hackerStory.Time < -62135596800 || hackerStory.Time > 253402300799)
            {
                throw new InvalidOperationException("Time property is outside the accepted range.");
            }

            return new Story
            {
                Title = hackerStory.Title,
                Uri = hackerStory.Url,
                PostedBy = hackerStory.By,
                Time = DateTimeOffset.FromUnixTimeSeconds(hackerStory.Time),
                Score = hackerStory.Score,
                CommentCount = hackerStory.Kids  == null ? 0 : hackerStory.Kids.Length
            };
        }
    }
}
