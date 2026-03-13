using RedScraps.Receive;
using RedScraps.Sent;

namespace RedScraps.Map;

internal static class CommentMapper
{
    public static CommentSent MapToCommentSent(CommentsRec.PostInfo postInfo, CommentsRec.AllComments commentsData)
    {
        
        var postSource = postInfo.data?.children?.FirstOrDefault()?.data;
        
 
        string actualPostId = postSource?.id ?? string.Empty;

        var result = new CommentSent
        {
            id = postSource?.id,
            title = postSource?.title,
            author = postSource?.author,
            selftext = postSource?.selftext,
            subreddit = postSource?.subreddit,
            num_comments = postSource?.num_comments,
            permalink = postSource?.permalink,
            Comments = new List<CommentSent.Comment>()
        };

        // 2. Begin the recursive flattening of the comment tree
        if (commentsData.data != null)
        {
            FlattenTree(commentsData.data, result.Comments, actualPostId);
        }

        return result;
    }

    private static void FlattenTree(
        CommentsRec.AllComments.CommentListing? currentListing, 
        List<CommentSent.Comment> flatList, 
        string rootPostId)
    {
        if (currentListing?.children == null) return;

        foreach (var child in currentListing.children)
        {

            if (child.kind == "t1" && child.data != null)
            {
                var data = child.data;


                bool isDirectReply = data.parent_id?.StartsWith("t3_") ?? false;

                flatList.Add(new CommentSent.Comment
                {
                    Author = data.author,
                    Body = data.body,
                    PostID = rootPostId,
               
                    ParentID = isDirectReply ? rootPostId : data.parent_id
                });

                
                if (data.replies != null)
                {
                    FlattenTree(data.replies, flatList, rootPostId);
                }
            }
        }
    }
}