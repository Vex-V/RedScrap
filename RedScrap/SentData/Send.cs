namespace RedScraps.Sent;

public class HomeSent
{
    public required string Subreddit { get; set; }
    public required string FirstID { get; set; }
    public required string LastID { get; set; }
    public required int TotalPosts { get; set; }

    public List<Post>? Posts;
    public class Post
    {
        public string? Author { get; set; }
        public string? PostID { get; set; }
        public string? SelfText { get; set; }
        public string? Title { get; set; }
        public string? Link { get; set; }
    }
}


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

public interface IUserData
{
    string Username { get; set; }
    string FirstID { get; set; }
    string LastID { get; set; }
    int TotalCount { get; set; }

}
public class UserSubmittedSent : IUserData
{
    public required string Username { get; set; }
    public required string FirstID { get; set; }
    public required string LastID { get; set; }
    public required int TotalCount { get; set; }

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
public class UserCommentsSent : IUserData
{
    public required string Username { get; set; }
    public required string FirstID { get; set; }
    public required string LastID { get; set; }
    public required int TotalCount { get; set; }

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