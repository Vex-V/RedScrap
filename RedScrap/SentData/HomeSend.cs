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