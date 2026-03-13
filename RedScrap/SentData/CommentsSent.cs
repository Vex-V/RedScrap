namespace RedScraps.Sent;

public class CommentSent
{   
    public string? id { get; set; }
    public string? title { get; set; }
    public string? author { get; set; }
    public string? selftext { get; set; }
    public string? subreddit { get; set; }
    public int? num_comments { get; set; }
    public string? permalink { get; set; }
    public List<Comment>? Comments;
    public class Comment
    {
        public string? Author { get; set; }
        public string? PostID { get; set; }
        public string? ParentID { get; set; }
        public string? Body { get; set; }
    }

}