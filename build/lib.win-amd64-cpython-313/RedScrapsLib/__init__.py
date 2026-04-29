import os
import sys
import time
from pythonnet import load

# Load .NET runtime
try:
    load("coreclr")
except Exception:
    pass

import clr

# Resolve DLL path
current_dir = os.path.dirname(os.path.abspath(__file__))
dll_name = "RedScrap.dll"
dll_path = os.path.join(current_dir, dll_name)

if current_dir not in sys.path:
    sys.path.append(current_dir)

if not os.path.exists(dll_path):
    raise FileNotFoundError(
        f"Missing DLL: {dll_path}\n"
        "Ensure the DLL and its .deps.json are in the same folder."
    )

clr.AddReference(dll_path)

try:
    from RedScraps import Scraper
except ImportError as e:
    raise ImportError(
        f"Could not find class 'Scraper' in namespace 'RedScraps' in {dll_name}."
    ) from e


# ----------------------------
# Internal state
# ----------------------------
_scraper_instance = None

_session_stats = {
    'calls': 0,
    'rate_limit_hits': 0,
    'total_wait_seconds': 0.0,
}


def _ensure_initialized():
    if _scraper_instance is None:
        raise RuntimeError(
            "Scraper not initialized. Call init(user_agent, debug) first."
        )


def _parse_rate_limit_wait(exc) -> float | None:
    """Return seconds to wait if exc is a rate limit error, else None."""
    msg = str(exc)
    if 'RateLimited:' not in msg:
        return None
    try:
        raw = msg.split('RateLimited:')[1].split(')')[0].strip()
        return float(raw)
    except (IndexError, ValueError):
        return 60.0


def _call_with_retry(label: str, task_fn):
    """
    Calls task_fn() (returns a C# Task), awaits .Result, and retries
    automatically on HTTP 429 rate limiting. Other errors return None.
    """
    while True:
        try:
            _ensure_initialized()
            result = task_fn().Result
            _session_stats['calls'] += 1
            return result
        except Exception as e:
            wait = _parse_rate_limit_wait(e)
            if wait is not None:
                _session_stats['rate_limit_hits'] += 1
                _session_stats['total_wait_seconds'] += wait
                print(
                    f"[RedScrapsLib] Rate limited on {label}. "
                    f"Waiting {wait:.0f}s... "
                    f"(hit #{_session_stats['rate_limit_hits']}, "
                    f"{_session_stats['total_wait_seconds']:.0f}s waited total)"
                )
                time.sleep(wait)
                # loop → retry automatically
            else:
                print(f"RedScraps Wrapper Error [{label}]: {e}")
                return None


# ----------------------------
# Public API
# ----------------------------

def init(user_agent=None, debug=False):
    """Initialize the Scraper instance. Must be called before any other function."""
    global _scraper_instance
    _scraper_instance = Scraper(user_agent, debug)


def get_stats() -> dict:
    """Return session statistics: total calls, rate limit hits, and total wait time."""
    return dict(_session_stats)


def get_home(subreddit, sort="hot", limit=100, time=None, after=None):
    """Fetch subreddit posts. Returns HomeSent or None."""
    return _call_with_retry(
        "get_home",
        lambda: _scraper_instance.ScrapHome(subreddit, sort, limit, time, after)
    )


def get_comments(subreddit, post_id, sort="confidence", limit=100):
    """Fetch post comments. Returns CommentSent or None."""
    return _call_with_retry(
        "get_comments",
        lambda: _scraper_instance.ScrapComments(subreddit, post_id, sort, limit)
    )


def get_user_posts(user, sort=None, limit=None, time=None, after=None):
    """Fetch user submissions. Returns UserSubmittedSent or None."""
    return _call_with_retry(
        "get_user_posts",
        lambda: _scraper_instance.ScrapUserData(user, sort, limit, time, after)
    )


def get_user_comments(user, sort=None, limit=None, time=None, after=None):
    """Fetch user comments. Returns UserCommentsSent or None."""
    return _call_with_retry(
        "get_user_comments",
        lambda: _scraper_instance.ScrapUserComments(user, sort, limit, time, after)
    )
