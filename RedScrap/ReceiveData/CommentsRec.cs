using System.Text.Json;
using System.Text.Json.Serialization;
namespace RedScraps.Receive;

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


