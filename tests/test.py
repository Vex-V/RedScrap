import sys
import os

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import RedScrapsLib as rs

PASS = "[PASS]"
FAIL = "[FAIL]"


def section(title):
    print(f"\n{'=' * 50}")
    print(f"  {title}")
    print('=' * 50)


def check(label, value, expected_type=None):
    if value is None:
        print(f"  {FAIL} {label}: got None")
        return False
    if expected_type and not isinstance(value, expected_type):
        print(f"  {FAIL} {label}: expected {expected_type.__name__}, got {type(value).__name__}")
        return False
    print(f"  {PASS} {label}: {repr(value)[:80]}")
    return True


# ── init ──────────────────────────────────────────────────────────────────────
section("init()")
rs.init(user_agent="RedScrapsTest/1.0", debug=False)
print(f"  {PASS} Scraper initialized")


# ── get_home ──────────────────────────────────────────────────────────────────
section("get_home('python', limit=3)")
home = rs.get_home("python", limit=3)

if home is None:
    print(f"  {FAIL} Returned None")
else:
    check("Subreddit",  home.Subreddit,  str)
    check("FirstID",    home.FirstID,    str)
    check("LastID",     home.LastID,     str)
    check("TotalPosts", home.TotalPosts)

    first_post_id = None
    if home.Posts:
        post = home.Posts[0]
        first_post_id = post.PostID
        print(f"\n  First post:")
        check("  PostID", post.PostID, str)
        check("  Title",  post.Title,  str)
        check("  Author", post.Author, str)
        check("  Link",   post.Link,   str)
    else:
        print(f"  {FAIL} Posts list is empty or None")


# ── get_comments ──────────────────────────────────────────────────────────────
section("get_comments('python', post_id, limit=5)")

if first_post_id:
    print(f"  Using post_id: {first_post_id}")
    comments = rs.get_comments("python", first_post_id, limit=5)

    if comments is None:
        print(f"  {FAIL} Returned None")
    else:
        check("PostID",       comments.PostID,       str)
        check("Title",        comments.Title,        str)
        check("Author",       comments.Author,       str)
        check("Subreddit",    comments.Subreddit,    str)
        check("Num_comments", comments.Num_comments)
        check("Permalink",    comments.Permalink,    str)

        if comments.Comments:
            c = comments.Comments[0]
            print(f"\n  First comment:")
            check("  CommentID", c.CommentID, str)
            check("  Author",    c.Author,    str)
            check("  Body",      c.Body,      str)
            check("  ParentID",  c.ParentID,  str)
        else:
            print(f"  {FAIL} Comments list is empty or None")
else:
    print(f"  SKIP — no post_id from get_home")


# ── get_user_posts ────────────────────────────────────────────────────────────
section("get_user_posts('spez', limit=3)")
user_posts = rs.get_user_posts("spez", limit=3)

if user_posts is None:
    print(f"  {FAIL} Returned None")
else:
    check("Username",   user_posts.Username,   str)
    check("FirstID",    user_posts.FirstID,    str)
    check("LastID",     user_posts.LastID,     str)
    check("TotalCount", user_posts.TotalCount)

    if user_posts.Posts:
        p = user_posts.Posts[0]
        print(f"\n  First post:")
        check("  PostID",       p.PostID,       str)
        check("  Title",        p.Title,        str)
        check("  Subreddit",    p.Subreddit,    str)
        check("  Upvotes",      p.Upvotes)
        check("  CommentCount", p.CommentCount)
        check("  CreatedUtc",   p.CreatedUtc)
    else:
        print(f"  {FAIL} Posts list is empty or None")


# ── get_user_comments ─────────────────────────────────────────────────────────
section("get_user_comments('spez', limit=3)")
user_comments = rs.get_user_comments("spez", limit=3)

if user_comments is None:
    print(f"  {FAIL} Returned None")
else:
    check("Username",   user_comments.Username,   str)
    check("FirstID",    user_comments.FirstID,    str)
    check("LastID",     user_comments.LastID,     str)
    check("TotalCount", user_comments.TotalCount)

    if user_comments.Comments:
        c = user_comments.Comments[0]
        print(f"\n  First comment:")
        check("  CommentID", c.CommentID, str)
        check("  Author",    c.Author,    str)
        check("  Subreddit", c.Subreddit, str)
        check("  Body",      c.Body,      str)
        check("  PostTitle", c.PostTitle, str)
        check("  Link",      c.Link,      str)
        check("  Upvotes",   c.Upvotes)
        check("  CreatedUtc", c.CreatedUtc)
    else:
        print(f"  {FAIL} Comments list is empty or None")


# ── get_stats() ───────────────────────────────────────────────────────────────
section("get_stats()")
stats = rs.get_stats()
check("calls",              stats['calls'],              int)
check("rate_limit_hits",    stats['rate_limit_hits'],    int)
check("total_wait_seconds", stats['total_wait_seconds'], float)

print(f"\n{'=' * 50}")
print("  Done.")
print('=' * 50)
