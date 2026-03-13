using System.Text;
namespace RedScraps.URLs;

public static class CommentURL
{
    public class CommentURLParameters
    {
        public const string BaseUrl = "https://www.reddit.com/r/";

        private string _subreddit = "";
        private string _postId = "";
        private string _sort = "confidence";
        private int _limit = 100;

        private static readonly HashSet<string> ValidSorts =
        [
            "confidence",
            "top",
            "new",
            "controversial",
            "old",
            "random",
            "qa"
        ];

        public string Subreddit
        {
            get => _subreddit;
            set => _subreddit = value?.Trim().Replace("/", "") ?? "";
        }

        public string PostId
        {
            get => _postId;
            set => _postId = value?.Trim() ?? "";
        }

        public string Sort
        {
            get => _sort;
            set
            {
                if (!string.IsNullOrEmpty(value) && ValidSorts.Contains(value.ToLower()))
                    _sort = value.ToLower();
                else
                    throw new ArgumentException($"Invalid Sort. Choose: {string.Join(", ", ValidSorts)}");
            }
        }

        public int Limit
        {
            get => _limit;
            set
            {
                if (value >= 1 && value <= 500)
                    _limit = value;
                else
                    throw new ArgumentOutOfRangeException(nameof(Limit), "Limit must be between 1 and 500.");
            }
        }

        public string FullUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Subreddit))
                    throw new InvalidOperationException("Subreddit must be set.");

                if (string.IsNullOrWhiteSpace(PostId))
                    throw new InvalidOperationException("PostId must be set.");

                StringBuilder url = new($"{BaseUrl}{Subreddit}/comments/{PostId}/.json");

                List<string> queryParams =
                [
                    $"sort={Sort}",
                    $"limit={Limit}"
                ];

                url.Append("?" + string.Join("&", queryParams));

                return url.ToString();
            }
        }
    }

    public static string CreateCommentURL(
        string subreddit,
        string postId,
        string? sort = null,
        int? limit = null
    )
    {
        CommentURLParameters URL = new();

        URL.Subreddit = subreddit;
        URL.PostId = postId;

        if (sort != null) URL.Sort = sort;
        if (limit != null) URL.Limit = limit.Value;

        return URL.FullUrl;
    }
}