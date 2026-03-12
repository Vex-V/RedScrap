using System.Text.Json.Serialization;

namespace RedScraps.Responses;

public class UserSubmitted
{
    public string? kind { get; set; } 
    public SubmittedData? data { get; set; }

    public class SubmittedData
    {
        public List<PostChild>? children { get; set; }

        public class PostChild
        {
            public string? kind { get; set; } 
            public PostChildData? data { get; set; }

            public class PostChildData
            {
                public string? id { get; set; } 
                public string? title { get; set; }
                public string? author { get; set; }
                public string? selftext { get; set; }
                public string? subreddit { get; set; }
                public int? ups { get; set; }
                public int? num_comments { get; set; }
                public string? permalink { get; set; }
                public string? url { get; set; }
                public double created_utc { get; set; }
            }
        }
    }
}

public class UserComments
{
    public string? kind { get; set; }
    public CommentListData? data { get; set; }

    public class CommentListData
    {
        public List<CommentChild>? children { get; set; }

        public class CommentChild
        {
            public string? kind { get; set; } // "t1"
            public CommentChildData? data { get; set; }

            public class CommentChildData
            {
                public string? id { get; set; } 
                public string? author { get; set; }
                public string? body { get; set; }
                public string? subreddit { get; set; }
                public string? link_id { get; set; } 
                public string? link_title { get; set; } 
                public string? parent_id { get; set; }
                public int? ups { get; set; }
                public double created_utc { get; set; }
                public string? permalink { get; set; }

                [JsonConverter(typeof(RedditReplyConverter))]
                public Comments.AllComments.CommentListing? replies { get; set; }
            }
        }
    }
}