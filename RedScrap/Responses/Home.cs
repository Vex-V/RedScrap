namespace RedScraps.Responses;

public class Home
{
    public string? kind { get; set; }
    public Data? data { get; set; }

    public class Data
    {
        public string? after { get; set; }
        public int? dist { get; set; }
        public List<Children>? children { get; set; }

        public class Children
        {
            public string? kind { get; set; }
            public Child_Data? data { get; set; }

            public class Child_Data
            {
                public string? subreddit { get; set; }
                public string? selftext { get; set; }
                public string? author { get; set; }
                public string? permalink { get; set; }
                public int? num_comments { get; set; }
            }
        }
    }
}