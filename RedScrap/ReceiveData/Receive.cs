namespace RedScraps.Receive;
using System.Text.Json.Serialization;

internal class HomeRec
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
                public string? title { get; set; }
                public string? selftext { get; set; }
                public string? author { get; set; }
                public string? permalink { get; set; }
                public int? num_comments { get; set; }
                public double? created_utc { get; set; }
                public string? Id { get; set;}
            }
        }
    }
}



internal class CommentsRec
{
    public class PostInfo
    {
        public string? kind { get; set; }
        public PostData? data { get; set; }

        public class PostData
        {
            public List<PostChild>? children { get; set; }

            public class PostChild
            {
                public PostChildData? data { get; set; }

                public class PostChildData
                {
                    public string? title { get; set; }
                    public string? author { get; set; }
                    public string? selftext { get; set; }
                    public string? subreddit { get; set; }
                    public int? ups { get; set; }
                    public int? num_comments { get; set; }
                    public string? permalink { get; set; }
                    public string? url { get; set; }
                    public string? id { get; set; }

                }
            }
        }
    }


    public class AllComments
    {
        public string? kind { get; set; }

        [JsonConverter(typeof(RedditReplyConverter))]
        public CommentListing? data { get; set; }

        public class CommentListing
        {
            public List<CommentChild>? children { get; set; }

            public class CommentChild
            {
                public string? kind { get; set; } 
                public CommentChildData? data { get; set; }

                public class CommentChildData
                {
                    public string? author { get; set; }
                    public string? body { get; set; }
                    public string? parent_id { get; set; }
                    public int? ups { get; set; }
                    public double? created_utc { get; set; }
                    
                    
                    [JsonConverter(typeof(RedditReplyConverter))]
                    public CommentListing? replies { get; set; } 
                }
            }
        }
    }
}


internal class UserSubmittedRec
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

internal class UserCommentsRec
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
                public CommentsRec.AllComments.CommentListing? replies { get; set; }
            }
        }
    }
}


