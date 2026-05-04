
# --- Sub Home posts ---
class HomeSent:
    Subreddit: str
    FirstID: str
    LastID: str
    TotalPosts: int
    Posts: Optional[List['HomeSent.Post']]

    class Post:
        Author: Optional[str]
        PostID: Optional[str]
        SelfText: Optional[str]
        Title: Optional[str]
        Link: Optional[str]

# --- Post Comments ---
class CommentSent:
    PostID: Optional[str]
    Title: Optional[str]
    Author: Optional[str]
    Selftext: Optional[str]
    Subreddit: Optional[str]
    Num_comments: Optional[int]
    Permalink: Optional[str]
    Comments: Optional[List['CommentSent.Comment']]

    class Comment:
        Author: Optional[str]
        CommentID: Optional[str]
        ParentID: Optional[str]
        Body: Optional[str]


class IUserData(Protocol):
    Username: str
    FirstID: str
    LastID: str
    TotalCount: int

# --- User Posts --- 
class UserSubmittedSent(IUserData):
    Username: str
    FirstID: str
    LastID: str
    TotalCount: int
    Posts: Optional[List['UserSubmittedSent.Post']]

    class Post:
        PostID: Optional[str]
        Title: Optional[str]
        Author: Optional[str]
        Subreddit: Optional[str]
        SelfText: Optional[str]
        Link: Optional[str]
        Upvotes: Optional[int]
        CommentCount: Optional[int]
        CreatedUtc: float


# --- User Comments --- 
class UserCommentsSent(IUserData):
    Username: str
    FirstID: str
    LastID: str
    TotalCount: int
    Comments: Optional[List['UserCommentsSent.Comment']]

    class Comment:
        CommentID: Optional[str]
        Author: Optional[str]
        Subreddit: Optional[str]
        Body: Optional[str]
        ParentID: Optional[str]
        PostID: Optional[str]
        PostTitle: Optional[str]
        Link: Optional[str]
        Upvotes: Optional[int]
        CreatedUtc: float