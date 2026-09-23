# RedScrapsLib

A Python library for scraping Reddit data — posts, comments, and user activity — without needing the official API. The scraping logic is written in C# (.NET 10) and exposed to Python via [pythonnet](https://github.com/pythonnet/pythonnet), with automatic rate-limit handling built in.

![PyPI](https://img.shields.io/pypi/v/redscrapslib)
![Python](https://img.shields.io/pypi/pyversions/redscrapslib)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20macOS%20%7C%20Linux-blue)
![License](https://img.shields.io/github/license/Vex-V/RedScrap)

---

## Requirements

| Requirement | Details |
|---|---|
| Python | 3.10+ |
| .NET Runtime | [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) — must be installed separately |
| Platform | Windows x64, macOS 12+ (Apple Silicon & Intel), Linux x86_64 (incl. WSL2) |

> **Note:** pip installs the Python wrapper and the compiled .NET assembly, but cannot install the .NET runtime itself. Download and install it from the link above before using the library.

---

## Installation

```bash
pip install redscrapslib
```

---

## Quick Start

Reddit requires authenticated requests — unauthenticated scraping gets rate-limited almost immediately. You must pass your Reddit session cookies to `init()`.

**Option A — export a `cookies.txt` file** from your browser (e.g. using the [Get cookies.txt LOCALLY](https://chromewebstore.google.com/detail/get-cookiestxt-locally/cclelndahbckbenkjhflpdbgdldlbecc) extension):

```python
import RedScrapsLib as rs

def parse_netscape_cookies(path, domain=None):
    cookies = {}
    with open(path, encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith('#'):
                continue
            parts = line.split('\t')
            if len(parts) != 7:
                continue
            c_domain, _, _, _, _, name, value = parts
            if domain is None or domain in c_domain:
                cookies[name] = value
    return cookies

cookies = parse_netscape_cookies("cookies.txt", domain="reddit.com")
rs.init(user_agent="MyBot/1.0", cookies=cookies)
```

**Option B — load directly from your browser** with [`browser_cookie3`](https://github.com/borisbabic/browser_cookie3) (`pip install browser_cookie3`):

```python
import RedScrapsLib as rs
import browser_cookie3

cj = browser_cookie3.firefox(domain_name='.reddit.com')
# cj = browser_cookie3.chrome(domain_name='.reddit.com')

rs.init(
    user_agent="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
    cookies=cj,
)
```

Once initialised, all four scrape functions work the same way:

```python
# Fetch posts from a subreddit
posts = rs.get_home("python", limit=10)
for post in posts.Posts:
    print(post.Title, post.Author)

# Fetch comments on a specific post
comments = rs.get_comments("python", post_id="abc123", limit=50)
for comment in comments.Comments:
    print(comment.Author, comment.Body)

# Fetch a user's post submissions
submissions = rs.get_user_posts("spez", limit=25)
for post in submissions.Posts:
    print(post.Title, post.Subreddit)

# Fetch a user's comments
user_comments = rs.get_user_comments("spez", limit=25)
for comment in user_comments.Comments:
    print(comment.Body, comment.Subreddit)

# Check session statistics
print(rs.get_stats())
# {'calls': 4, 'rate_limit_hits': 0, 'total_wait_seconds': 0.0}
```

---

## Rate Limiting

Reddit's unofficial API enforces a hard limit of roughly **100 requests per window**. RedScrapsLib handles this automatically — no extra code needed.

When a 429 response is received, the library:
1. Reads the `Retry-After` header (defaults to 60s if absent)
2. Prints a message so you know it's waiting
3. Sleeps for the required time
4. Retries the request transparently

```
[RedScrapsLib] Rate limited on get_home. Waiting 60s... (hit #1, 60s waited total)
```

This means you can run long loops without worrying about crashes:

```python
rs.init(user_agent="MyBot/1.0")

for subreddit in my_list:
    data = rs.get_home(subreddit)  # sleeps and retries automatically if rate limited
    process(data)

print(rs.get_stats())
# {'calls': 250, 'rate_limit_hits': 3, 'total_wait_seconds': 780.0}
```

> **Based on testing:** Reddit allows ~100 requests before rate limiting, then applies ~480s penalties for sustained hammering. For bulk scraping, adding a small delay between calls avoids the heavy penalty entirely.

---

## API Reference

### `init(user_agent=None, debug=False, cookies=None)`

Initialises the scraper. Must be called once before any other function.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `user_agent` | `str \| None` | `None` | Custom User-Agent string sent with every request. Defaults to `"RedScrapsBot"` |
| `debug` | `bool` | `False` | Prints step-by-step logs for each request when `True` |
| `cookies` | `dict[str, str] \| CookieJar \| None` | `None` | Cookies to attach to every request. Accepts a plain `{name: value}` dict or a `CookieJar` (e.g. from `browser_cookie3`) |

---

### `get_home(subreddit, sort="hot", limit=100, time=None, after=None) → HomeSent`

Fetches posts from a subreddit.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `subreddit` | `str` | — | Subreddit name (without `r/`) |
| `sort` | `str` | `"hot"` | `"hot"`, `"new"`, `"top"`, `"rising"` |
| `limit` | `int` | `100` | Number of posts to fetch (max 100 per request) |
| `time` | `str \| None` | `None` | Time filter for `"top"`: `"hour"`, `"day"`, `"week"`, `"month"`, `"year"`, `"all"` |
| `after` | `str \| None` | `None` | Post ID to paginate from |

**Returns: `HomeSent`**

```
HomeSent
├── Subreddit     str
├── FirstID       str
├── LastID        str          ← use as `after` to paginate
├── TotalPosts    int
└── Posts         List[Post]
    ├── PostID    str | None
    ├── Title     str | None
    ├── Author    str | None
    ├── SelfText  str | None
    └── Link      str | None
```

---

### `get_comments(subreddit, post_id, sort="confidence", limit=100) → CommentSent`

Fetches comments for a specific post.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `subreddit` | `str` | — | Subreddit the post belongs to |
| `post_id` | `str` | — | Post ID (e.g. `"abc123"`) |
| `sort` | `str` | `"confidence"` | `"confidence"`, `"top"`, `"new"`, `"controversial"`, `"old"` |
| `limit` | `int` | `100` | Max number of comments to fetch |

**Returns: `CommentSent`**

```
CommentSent
├── PostID        str | None
├── Title         str | None
├── Author        str | None
├── Selftext      str | None
├── Subreddit     str | None
├── Num_comments  int | None
├── Permalink     str | None
└── Comments      List[Comment]
    ├── CommentID str | None
    ├── Author    str | None
    ├── ParentID  str | None
    └── Body      str | None
```

---

### `get_user_posts(user, sort=None, limit=None, time=None, after=None) → UserSubmittedSent`

Fetches a user's post submissions.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `user` | `str` | — | Reddit username (without `u/`) |
| `sort` | `str \| None` | `None` | `"hot"`, `"new"`, `"top"`, `"controversial"` |
| `limit` | `int \| None` | `None` | Number of posts to fetch |
| `time` | `str \| None` | `None` | Time filter when using `"top"` |
| `after` | `str \| None` | `None` | Post ID to paginate from |

**Returns: `UserSubmittedSent`**

```
UserSubmittedSent
├── Username      str
├── FirstID       str
├── LastID        str          ← use as `after` to paginate
├── TotalCount    int
└── Posts         List[Post]
    ├── PostID       str | None
    ├── Title        str | None
    ├── Author       str | None
    ├── Subreddit    str | None
    ├── SelfText     str | None
    ├── Link         str | None
    ├── Upvotes      int | None
    ├── CommentCount int | None
    └── CreatedUtc   float
```

---

### `get_user_comments(user, sort=None, limit=None, time=None, after=None) → UserCommentsSent`

Fetches a user's comment history.

| Parameter | Type | Default | Description |
|---|---|---|---|
| `user` | `str` | — | Reddit username (without `u/`) |
| `sort` | `str \| None` | `None` | `"hot"`, `"new"`, `"top"`, `"controversial"` |
| `limit` | `int \| None` | `None` | Number of comments to fetch |
| `time` | `str \| None` | `None` | Time filter when using `"top"` |
| `after` | `str \| None` | `None` | Comment ID to paginate from |

**Returns: `UserCommentsSent`**

```
UserCommentsSent
├── Username      str
├── FirstID       str
├── LastID        str          ← use as `after` to paginate
├── TotalCount    int
└── Comments      List[Comment]
    ├── CommentID  str | None
    ├── Author     str | None
    ├── Subreddit  str | None
    ├── Body       str | None
    ├── ParentID   str | None
    ├── PostID     str | None
    ├── PostTitle  str | None
    ├── Link       str | None
    ├── Upvotes    int | None
    └── CreatedUtc float
```

---

### `get_stats() → dict`

Returns session statistics since `init()` was called.

```python
{
    'calls': int,               # total successful API calls
    'rate_limit_hits': int,     # number of 429 responses received
    'total_wait_seconds': float # total time spent waiting on rate limits
}
```

---

## Pagination

Every response includes `FirstID` and `LastID`. Pass `LastID` as the `after` parameter to fetch the next page:

```python
rs.init(user_agent="MyBot/1.0")

after = None
all_posts = []

while True:
    page = rs.get_home("python", limit=100, after=after)
    all_posts.extend(page.Posts)

    if page.TotalPosts < 100:
        break  # last page

    after = page.LastID
```

---

## Architecture

```
Python (RedScrapsLib)
    │
    │  pythonnet
    ▼
C# .NET 10 Assembly (RedScrap.dll)
    ├── Scraper          — HttpClient, request logic
    ├── URLs             — URL builders for each endpoint
    ├── Receive (JSON)   — deserialisation models
    ├── Map              — raw → clean data mapping
    └── Sent             — clean data models returned to Python
```
