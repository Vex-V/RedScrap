using System.Text;
namespace RedScraps.URLs;

public static class HomeURL
{
   
    public class HomeURLParameters
    {
        public const string BaseUrl = "https://www.reddit.com/r/";

        private string _subreddit = "";
        private string _sort = "hot";
        private int _limit = 100;
        private string _time = "";
        private string _after = "";

        private static readonly HashSet<string> ValidSorts = ["best", "top", "hot", "controversial", "new"];
        private static readonly HashSet<string> ValidTimes = ["hour", "day", "week", "month", "year", "all"];

        public string Subreddit
        {
            get => _subreddit;
            set => _subreddit = value?.Trim().Replace("/", "") ?? "";
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
                if (value >= 1 && value <= 100)
                    _limit = value;
                else
                    throw new ArgumentOutOfRangeException(nameof(Limit), "Limit must be between 1 and 100.");
            }
        }

        public string Time
        {
            get => _time;
            set
            {
                if (_sort != "top" && _sort != "controversial")
                    throw new InvalidOperationException("Time filter requires Sort to be 'top' or 'controversial'.");

                if (!string.IsNullOrEmpty(value) && ValidTimes.Contains(value.ToLower()))
                    _time = value.ToLower();
                else
                    throw new ArgumentException($"Invalid Time. Choose: {string.Join(", ", ValidTimes)}");
            }
        }

        public string After
        {
            get => _after;
            set => _after = value ?? "";
        }

        public string FullUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Subreddit))
                    throw new InvalidOperationException("Subreddit must be set before generating a URL.");

                StringBuilder url = new($"{BaseUrl}{Subreddit}/");
                if (!string.IsNullOrEmpty(Sort))
                {
                    url.Append($"{Sort}/");
                }
                url.Append(".json");

                List<string> queryParams = [$"limit={Limit}"];

                if (!string.IsNullOrEmpty(Time)) 
                    queryParams.Add($"t={Time}");

                if (!string.IsNullOrEmpty(After)) 
                    queryParams.Add($"after={After}");

                url.Append("?" + string.Join("&", queryParams));

                return url.ToString();
            }
        }
    }

   
    public static string CreateHomeURL(
        string subreddit, 
        string? sort = null, 
        int? limit = null, 
        string? time = null, 
        string? after = null
    )
    {
        HomeURLParameters URL = new();

        URL.Subreddit = subreddit;
        if (sort != null) URL.Sort = sort;
        if (limit != null) URL.Limit = limit.Value;
        if (time != null) URL.Time = time;
        if (after != null) URL.After = after;

        return URL.FullUrl;
    }
}