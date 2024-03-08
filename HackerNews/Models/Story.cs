namespace HackerNews.Models
{
    public class Story
    {
        public string Title { get; set; } = String.Empty;

        public string Uri { get; set; } = String.Empty;

        public string PostedBy { get; set; } = String.Empty;

        public DateTimeOffset? Time { get; set; }

        public int Score { get; set; }

        public int CommentCount { get; set; }
    }
}
