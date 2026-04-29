# RedScrapsLib

A Python library for scraping Reddit data (posts, comments, user activity) without the official API. Backed by a .NET 9 assembly via pythonnet, with automatic rate-limit handling.

## Requirements

- Python 3.10+
- Windows x64
- [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) — must be installed separately; pip cannot install it

## Installation

```bash
pip install redscrapslib
```

## Quick start

```python
import RedScrapsLib as rs

rs.init(user_agent="MyBot/1.0")

# Subreddit posts
posts = rs.get_home("python", limit=10)
for post in posts.Posts:
    print(post.Title, post.Author)

# Post comments
comments = rs.get_comments("python", post_id="abc123", limit=50)

# User activity
submissions = rs.get_user_posts("spez", limit=25)
user_comments = rs.get_user_comments("spez", limit=25)

# Session stats
print(rs.get_stats())
# {'calls': 4, 'rate_limit_hits': 0, 'total_wait_seconds': 0.0}
```

Rate limiting is handled automatically. If Reddit returns a 429, the library waits the required time and retries — no extra code needed.

## API

### `init(user_agent=None, debug=False)`
Must be called once before any other function.

### `get_home(subreddit, sort="hot", limit=100, time=None, after=None) → HomeSent`

### `get_comments(subreddit, post_id, sort="confidence", limit=100) → CommentSent`

### `get_user_posts(user, sort=None, limit=None, time=None, after=None) → UserSubmittedSent`

### `get_user_comments(user, sort=None, limit=None, time=None, after=None) → UserCommentsSent`

### `get_stats() → dict`
Returns `{'calls': int, 'rate_limit_hits': int, 'total_wait_seconds': float}`.
