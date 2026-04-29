# RedScrapsLib/__init__.pyi

from typing import List, Optional, Protocol

# --- Wrapper Functions ---

def init(user_agent: Optional[str] = ..., debug: bool = ...) -> None: 
    """Initialize the underlying .NET Scraper instance."""
    ...

def get_home(
    subreddit: str, 
    sort: str = ..., 
    limit: int = ..., 
    time: Optional[str] = ..., 
    after: Optional[str] = ...
) -> Optional[HomeSent]: 
    """Fetch subreddit posts. Returns HomeSent or None."""
    ...

def get_comments(
    subreddit: str, 
    post_id: str, 
    sort: str = ..., 
    limit: int = ...
) -> Optional[CommentSent]: 
    """Fetch post comments. Returns CommentSent or None."""
    ...

def get_user_posts(
    user: str, 
    sort: Optional[str] = ..., 
    limit: Optional[int] = ..., 
    time: Optional[str] = ..., 
    after: Optional[str] = ...
) -> Optional[UserSubmittedSent]: 
    """Fetch user submissions. Returns UserSubmittedSent or None."""
    ...

def get_user_comments(
    user: str, 
    sort: Optional[str] = ..., 
    limit: Optional[int] = ..., 
    time: Optional[str] = ..., 
    after: Optional[str] = ...
) -> Optional[UserCommentsSent]: 
    """Fetch user comments. Returns UserCommentsSent or None."""
    ...

# --- Data Types (Classes) ---

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