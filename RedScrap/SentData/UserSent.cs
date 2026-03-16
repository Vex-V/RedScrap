namespace RedScraps.Sent;

public class UserSubmittedSent
{
    public required string Username { get; set; }
    public required string FirstID { get; set; }
    public required string LastID { get; set; }
    public required int TotalPosts { get; set; }

    public List<Post>? Posts;

    public class Post
    {
        public string? PostID { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Subreddit { get; set; }
        public string? SelfText { get; set; }
        public string? Link { get; set; }
        public int? Upvotes { get; set; }
        public int? CommentCount { get; set; }
        public double CreatedUtc { get; set; }
    }
}
public class UserCommentsSent
{
    public required string Username { get; set; }
    public required string FirstID { get; set; }
    public required string LastID { get; set; }
    public required int TotalComments { get; set; }

    public List<Comment>? Comments;

    public class Comment
    {
        public string? CommentID { get; set; }
        public string? Author { get; set; }
        public string? Subreddit { get; set; }
        public string? Body { get; set; }

        public string? ParentID { get; set; }
        public string? PostID { get; set; }
        public string? PostTitle { get; set; }

        public string? Link { get; set; }
        public int? Upvotes { get; set; }

        public double CreatedUtc { get; set; }
    }
}