namespace HackerNews.Models
{
    public class HackerStory
    {
        public string By { get; set; } = String.Empty;

        public int Descendants { get; set; }

        public int Id { get; set; }

        public int[]? Kids { get; set; }

        public int Score { get; set; }

        public long Time { get; set; }

        public string Title { get; set; } = String.Empty;

        public string Type { get; set; } = String.Empty;

        public string Url { get; set; } = String.Empty;
    }
}
