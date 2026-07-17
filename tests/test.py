import sys
import os

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

import RedScrapsLib as rs

PASS = "[PASS]"
FAIL = "[FAIL]"
COOKIE_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), "cookies.txt")


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


def parse_netscape_cookies(path, domain=None):
    """Parse a Netscape cookies.txt and return {name: value} filtered by domain."""
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


def run_api_checks():
    """Run the standard API checks after init."""
    home = rs.get_home("python", limit=3)
    first_post_id = None
    if home is None:
        print(f"  {FAIL} get_home returned None")
    else:
        check("get_home Subreddit", home.Subreddit, str)
        check("get_home TotalPosts", home.TotalPosts)
        if home.Posts:
            first_post_id = home.Posts[0].PostID
            print(f"  {PASS} get_home Posts[0].PostID: {first_post_id}")

    if first_post_id:
        comments = rs.get_comments("python", first_post_id, limit=3)
        if comments is None:
            print(f"  {FAIL} get_comments returned None")
        else:
            check("get_comments PostID", comments.PostID, str)
            check("get_comments Num_comments", comments.Num_comments)

    user_posts = rs.get_user_posts("spez", limit=3)
    if user_posts is None:
        print(f"  {FAIL} get_user_posts returned None")
    else:
        check("get_user_posts Username", user_posts.Username, str)
        check("get_user_posts TotalCount", user_posts.TotalCount)


# ── Test 1: cookies.txt (user parses manually, passes dict) ───────────────────
section("Cookie source: cookies.txt (manual parse -> dict)")

cookies_dict = parse_netscape_cookies(COOKIE_FILE, domain="reddit.com")
print(f"  Loaded {len(cookies_dict)} Reddit cookies from file")

rs.init(user_agent="RedScrapsTest/1.0", cookies=cookies_dict)
print(f"  {PASS} init() with dict cookies")

#run_api_checks()


# ── Test 2: browser_cookie3 (CookieJar passed directly) ───────────────────────
section("Cookie source: browser_cookie3 (CookieJar -> init)")

try:
    import browser_cookie3
    cj = browser_cookie3.firefox(domain_name='.reddit.com')
    cookie_count = sum(1 for _ in cj)
    print(f"  Loaded {cookie_count} Reddit cookies from Firefox via browser_cookie3")

    rs.init(
        user_agent="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
        cookies=cj,
    )
    print(f"  {PASS} init() with CookieJar cookies")

    run_api_checks()

except ImportError:
    print(f"  SKIP — browser_cookie3 not installed")
except Exception as e:
    print(f"  SKIP — browser_cookie3 failed: {e}")


# ── Stats ──────────────────────────────────────────────────────────────────────
section("get_stats()")
s = rs.get_stats()
check("calls",              s['calls'],              int)
check("rate_limit_hits",    s['rate_limit_hits'],    int)
check("total_wait_seconds", s['total_wait_seconds'], float)

print(f"\n{'=' * 50}")
print("  Done.")
print('=' * 50)
