using System.Text;
using System;
namespace RedScraps.URLs;

internal static class UserURL
{
    public class UserURLParameters
    {
        public const string BaseUrl = "https://www.reddit.com/user/";
        private string _user = "";
        private string _sort = "hot";
        private int _limit = 100;
        private string _time = "";
        private string _after = "";
        private string _type = "";
        private static readonly HashSet<string> ValidSorts = ["best", "top", "hot", "controversial", "new"];
        private static readonly HashSet<string> ValidTimes = ["hour", "day", "week", "month", "year", "all"];
        private static readonly HashSet<string> ValidTypes =["submitted", "comments"];
        
        public string User
        {
            get => _user;
            set => _user = value?.Trim().Replace("/", "") ?? "";
        }
        public string Type
        {
            get => _type;
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && ValidTypes.Contains(value.ToLower()))
                    _type = value.ToLower();
                else
                    throw new ArgumentException($"Invalid Type. Choose: {string.Join(", ", ValidTypes)}");
            }
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
                if (string.IsNullOrWhiteSpace(User))
                    throw new InvalidOperationException("User must be set before generating a URL.");

                if (string.IsNullOrWhiteSpace(Type))
                    throw new InvalidOperationException("Type must be set (submitted/comments/etc).");

                StringBuilder url = new($"{BaseUrl}{User}/{Type}.json");

                List<string> queryParams = new()
                {
                    $"limit={Limit}"
                };

                if (!string.IsNullOrEmpty(Sort))
                    queryParams.Add($"sort={Sort}");

                if (!string.IsNullOrEmpty(Time))
                    queryParams.Add($"t={Time}");

                if (!string.IsNullOrEmpty(After))
                    queryParams.Add($"after={After}");

                url.Append("?" + string.Join("&", queryParams));

                return url.ToString();
            }
        }
    }

        public static string CreateUserURL(
        string user, 
        string type,
        string? sort = null, 
        int? limit = null, 
        string? time = null, 
        string? after = null
    )
    {
        UserURLParameters URL = new();

        URL.User = user;
        URL.Type = type;
        if (sort != null) URL.Sort = sort;
        if (limit != null) URL.Limit = limit.Value;
        if (time != null) URL.Time = time;
        if (after != null) URL.After = after;

        return URL.FullUrl;

    }
}

