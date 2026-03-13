namespace RedScraps.Sent;

public class CommentSent
{   
    public string? PostID { get; set; }
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Selftext { get; set; }
    public string? Subreddit { get; set; }
    public int? Num_comments { get; set; }
    public string? Permalink { get; set; }
    public List<Comment>? Comments;
    public class Comment
    {
        public string? Author { get; set; }
        public string? CommentID { get; set; }
        public string? ParentID { get; set; }
        public string? Body { get; set; }
    }

}