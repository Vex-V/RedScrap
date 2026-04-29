using System.Linq;
using RedScraps.Receive;
using RedScraps.Sent;

namespace RedScraps.Map;

internal static class UserMapper
{
    public static UserSubmittedSent MapToUserSubSent(UserSubmittedRec source)
    {
        var children = source.data?.children ?? new List<UserSubmittedRec.SubmittedData.PostChild>();

       
        var firstChild = children.FirstOrDefault()?.data;
        var lastChild = children.LastOrDefault()?.data;


        return new UserSubmittedSent
        {
            Username = firstChild?.author ?? string.Empty,
            FirstID = firstChild?.id ?? string.Empty,
            LastID = lastChild?.id ?? string.Empty,
            TotalCount = children.Count,

            Posts = children.Select(c => new UserSubmittedSent.Post
            {
                PostID = c.data?.id,
                Title = c.data?.title,
                Subreddit = c.data?.subreddit,
                SelfText = c.data?.selftext,
                Link = c.data?.permalink,
                Upvotes = c.data?.ups,
                CommentCount = c.data?.num_comments,
                CreatedUtc = c.data?.created_utc ?? 0.0

            }).ToList()
        };
    }

    public static UserCommentsSent MapToUserComSent (UserCommentsRec source)
    {

        var children = source.data?.children ?? new List<UserCommentsRec.CommentListData.CommentChild>();

        var firstChild = children.FirstOrDefault()?.data;
        var lastChild = children.LastOrDefault()?.data;

        return new UserCommentsSent
        {
            Username = firstChild?.author ?? string.Empty,
            FirstID = firstChild?.id ?? string.Empty,
            LastID = lastChild?.id ?? string.Empty,
            TotalCount = children.Count,

            Comments = children.Select(c => new UserCommentsSent.Comment
            {
                Author = c.data?.author,
                CommentID = c.data?.id,
                Subreddit = c.data?.subreddit,
                Body = c.data?.body,
                ParentID = c.data?.parent_id,
                PostID = c.data?.link_id,
                PostTitle = c.data?.link_title,
                Link = c.data?.permalink,
                Upvotes = c.data?.ups,
                CreatedUtc = c.data?.created_utc ?? 0.0
            }).ToList()

            
        };
    }
}