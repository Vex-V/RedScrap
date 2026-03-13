using System.Linq;
using RedScraps.Receive;
using RedScraps.Sent;

namespace RedScraps.Map;

internal static class PostMapper
{
    public static HomeSent MapToHomeSent(HomeRec source)
    {
       
        var children = source.data?.children ?? new List<HomeRec.Data.Children>();

       
        var firstChild = children.FirstOrDefault()?.data;
        var lastChild = children.LastOrDefault()?.data;

       
        return new HomeSent
        {
           
            Subreddit = firstChild?.subreddit ?? "N/A",
            FirstID = firstChild?.Id ?? string.Empty,
            LastID = lastChild?.Id ?? string.Empty,
            TotalPosts = children.Count,

            
            Posts = children.Select(c => new HomeSent.Post
            {
                Author = c.data?.author,
                PostID = c.data?.Id,
                SelfText = c.data?.selftext,
                Link = c.data?.permalink,
                Title = c.data?.title
            }).ToList()
        };
    }
}