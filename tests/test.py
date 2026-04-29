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
    check("Subreddit", home.Subreddit, str)
    check("FirstID",   home.FirstID,   str)
    check("LastID",    home.LastID,    str)
    check("TotalPosts", home.TotalPosts)

    first_post_id = None
    if home.Posts:
        post = home.Posts[0]
        first_post_id = post.PostID
        print(f"\n  First post:")
        check("  PostID",   post.PostID,   str)
        check("  Title",    post.Title,    str)
        check("  Author",   post.Author,   str)
        check("  Link",     post.Link,     str)
    else:
        print(f"  {FAIL} Posts list is empty or None")


# ── get_comments ───────────────────────────────────────────────────────────────
section(f"get_comments('python', post_id, limit=5)")

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


# ── get_user_posts ─────────────────────────────────────────────────────────────
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


# ── get_user_comments ──────────────────────────────────────────────────────────
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


# ── Rate limit auto-retry ──────────────────────────────────────────────────────
section("Rate Limit Auto-Retry (mock: 2 failures, 2s wait each)")

import time as _time

FAIL_COUNT = 2
WAIT_SECS  = 2

class _RateLimitTask:
    """Simulates a faulted C# Task (HTTP 429)."""
    def __init__(self, wait_secs):
        self._msg = f"RateLimited:{wait_secs}"
    @property
    def Result(self):
        raise Exception(self._msg)

class _MockScraper:
    """Injects N rate-limit failures then delegates to the real scraper."""
    def __init__(self, real, fail_count, wait_secs):
        self._real       = real
        self._fail_count = fail_count
        self._wait_secs  = wait_secs
        self._attempts   = 0

    def ScrapHome(self, *args):
        self._attempts += 1
        if self._attempts <= self._fail_count:
            print(f"  [mock] attempt #{self._attempts} -> 429 rate limited")
            return _RateLimitTask(self._wait_secs)
        print(f"  [mock] attempt #{self._attempts} -> success")
        return self._real.ScrapHome(*args)

real_scraper    = rs._scraper_instance
mock            = _MockScraper(real_scraper, FAIL_COUNT, WAIT_SECS)
stats_before    = rs.get_stats()

rs._scraper_instance = mock
t0     = _time.time()
result = rs.get_home("python", limit=1)
elapsed = _time.time() - t0
rs._scraper_instance = real_scraper  # always restore

stats_after = rs.get_stats()
delta_hits  = stats_after['rate_limit_hits']     - stats_before['rate_limit_hits']
delta_wait  = stats_after['total_wait_seconds']  - stats_before['total_wait_seconds']

check("Returned real data after retries", result)

expected_hits = FAIL_COUNT
if delta_hits == expected_hits:
    print(f"  {PASS} rate_limit_hits delta: +{delta_hits} (expected {expected_hits})")
else:
    print(f"  {FAIL} rate_limit_hits delta: +{delta_hits} (expected {expected_hits})")

expected_wait = FAIL_COUNT * WAIT_SECS
if delta_wait >= expected_wait:
    print(f"  {PASS} total_wait_seconds delta: +{delta_wait:.1f}s (>= {expected_wait}s)")
else:
    print(f"  {FAIL} total_wait_seconds delta: +{delta_wait:.1f}s (expected >= {expected_wait}s)")

if elapsed >= expected_wait:
    print(f"  {PASS} Wall-clock time: {elapsed:.1f}s (actually waited)")
else:
    print(f"  {FAIL} Wall-clock time: {elapsed:.1f}s (should be >= {expected_wait}s)")


# ── get_stats() ───────────────────────────────────────────────────────────────
section("get_stats()")
stats = rs.get_stats()
check("calls",               stats['calls'],               int)
check("rate_limit_hits",     stats['rate_limit_hits'],     int)
check("total_wait_seconds",  stats['total_wait_seconds'],  float)

# ── Live rate limit trigger ────────────────────────────────────────────────────
section("Live Rate Limit Trigger (hammering API until Reddit pushes back)")

print("  Making rapid requests. The library will auto-retry on 429 — no action needed.\n")

MAX_CALLS   = 300
hits_before = rs.get_stats()['rate_limit_hits']
i           = 0

try:
    while i < MAX_CALLS:
        i += 1
        rs.get_home("python", limit=1)
        hits_now = rs.get_stats()['rate_limit_hits'] - hits_before
        print(f"  call #{i:<4}  |  rate-limit hits: {hits_now}", end="\r", flush=True)
        if hits_now >= 1:
            print()  # clear the \r line
            break
    else:
        print(f"\n  Reached {MAX_CALLS} calls with no rate limit — Reddit was lenient.")
except KeyboardInterrupt:
    print(f"\n  Stopped by user after {i} calls.")

print()
s = rs.get_stats()
hits_delta = s['rate_limit_hits'] - hits_before
if hits_delta:
    print(f"  {PASS} Rate limit triggered and recovered automatically")
    print(f"  {PASS} Calls before first 429: ~{i - hits_delta}")
    print(f"  {PASS} Rate-limit hits this loop: {hits_delta}")
    print(f"  {PASS} Total auto-wait: {s['total_wait_seconds']:.0f}s")
else:
    print(f"  (No rate limit hit in {i} calls — try again later or increase MAX_CALLS)")


print(f"\n{'=' * 50}")
print("  Done.")
print('=' * 50)
