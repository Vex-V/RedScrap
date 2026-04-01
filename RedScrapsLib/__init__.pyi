from typing import List, Optional, Union

# ----------------------------
# Initialization
# ----------------------------

def init(user_agent: Optional[str] = None, debug: bool = False) -> None:
    """
    Initialize the underlying Scraper instance.
    Must be called before using any other function.
    """
    ...


# ----------------------------
# HomeSent
# ----------------------------

class HomePost:
    Author: Optional[str]
    PostID: Optional[str]
    SelfText: Optional[str]
    Title: Optional[str]
    Link: Optional[str]


class HomeSent:
    Subreddit: str
    FirstID: str
    LastID: str
    TotalPosts: int
    Posts: Optional[List[HomePost]]


# ----------------------------
# CommentSent
# ----------------------------

class CommentNode:
    Author: Optional[str]
    CommentID: Optional[str]
    ParentID: Optional[str]
    Body: Optional[str]


class CommentSent:
    PostID: Optional[str]
    Title: Optional[str]
    Author: Optional[str]
    Selftext: Optional[str]
    Subreddit: Optional[str]
    Num_comments: Optional[int]
    Permalink: Optional[str]
    Comments: Optional[List[CommentNode]]


# ----------------------------
# IUserData (interface base)
# ----------------------------

class IUserData:
    Username: str
    FirstID: str
    LastID: str
    TotalCount: int


# ----------------------------
# UserSubmittedSent
# ----------------------------

class UserSubmittedPost:
    PostID: Optional[str]
    Title: Optional[str]
    Author: Optional[str]
    Subreddit: Optional[str]
    SelfText: Optional[str]
    Link: Optional[str]
    Upvotes: Optional[int]
    CommentCount: Optional[int]
    CreatedUtc: float


class UserSubmittedSent(IUserData):
    Posts: Optional[List[UserSubmittedPost]]


# ----------------------------
# UserCommentsSent
# ----------------------------

class UserComment:
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


class UserCommentsSent(IUserData):
    Comments: Optional[List[UserComment]]


# ----------------------------
# Wrapper functions
# ----------------------------

def get_home(
    subreddit: str,
    sort: str = "hot",
    limit: int = 100,
    time: Optional[str] = None,
    after: Optional[str] = None
) -> Optional[HomeSent]:
    """
    Fetch subreddit posts.
    """
    ...


def get_comments(
    subreddit: str,
    post_id: str,
    sort: str = "confidence",
    limit: int = 100
) -> Optional[CommentSent]:
    """
    Fetch flattened comments for a post.
    """
    ...


def get_user_data(
    user: str,
    type: str,
    sort: Optional[str] = None,
    limit: Optional[int] = None,
    time: Optional[str] = None,
    after: Optional[str] = None
) -> Optional[Union[UserSubmittedSent, UserCommentsSent]]:
    """
    Fetch user data.

    type:
        - "comments" → UserCommentsSent
        - "submitted" → UserSubmittedSent
    """
    ...