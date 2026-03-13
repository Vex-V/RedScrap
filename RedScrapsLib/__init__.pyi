from typing import List, Optional



class Post:
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
    Posts: Optional[List[Post]]

class Comment:
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
    Comments: Optional[List[Comment]]

# --- Wrapper Function Signatures ---

def get_home(
    subreddit: str, 
    sort: str = "hot", 
    limit: int = 100, 
    time: Optional[str] = None, 
    after: Optional[str] = None
) -> Optional[HomeSent]:
    """
    Fetches the home page posts for a subreddit. 
    Returns a HomeSent object with PascalCase properties.
    """
    ...

def get_comments(
    subreddit: str, 
    post_id: str, 
    sort: str = "confidence", 
    limit: int = 100
) -> Optional[CommentSent]:
    """
    Fetches the comment tree for a post. 
    Returns a CommentSent object containing a flattened list of PascalCase Comments.
    """
    ...