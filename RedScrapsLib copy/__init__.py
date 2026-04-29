import os
import sys
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

# Import correct class
try:
    from RedScraps import Scraper
except ImportError as e:
    raise ImportError(
        f"Could not find class 'Scraper' in namespace 'RedScraps' in {dll_name}."
    ) from e


# ----------------------------
# Internal instance (singleton)
# ----------------------------
_scraper_instance = None


def init(user_agent=None, debug=False):
    """
    Initialize the Scraper instance.
    
    Must be called before using any other function.
    """
    global _scraper_instance
    _scraper_instance = Scraper(user_agent, debug)


def _ensure_initialized():
    if _scraper_instance is None:
        raise RuntimeError(
            "Scraper not initialized. Call init(user_agent, debug) first."
        )


# ----------------------------
# Wrapper functions
# ----------------------------

def get_home(subreddit, sort="hot", limit=100, time=None, after=None):
    """
    Fetch subreddit posts.
    Returns HomeSent or None.
    """
    try:
        _ensure_initialized()
        task = _scraper_instance.ScrapHome(subreddit, sort, limit, time, after)
        return task.Result
    except Exception as e:
        print(f"RedScraps Wrapper Error [get_home]: {e}")
        return None


def get_comments(subreddit, post_id, sort="confidence", limit=100):
    """
    Fetch post comments.
    Returns CommentSent or None.
    """
    try:
        _ensure_initialized()
        task = _scraper_instance.ScrapComments(subreddit, post_id, sort, limit)
        return task.Result
    except Exception as e:
        print(f"RedScraps Wrapper Error [get_comments]: {e}")
        return None


def get_user_posts(user, sort=None, limit=None, time=None, after=None):
    """Fetch user submissions. Returns UserSubmittedSent or None."""
    try:
        _ensure_initialized()
        task = _scraper_instance.ScrapUserData(user, sort, limit, time, after)
        return task.Result
    except Exception as e:
        print(f"RedScraps Wrapper Error [get_user_posts]: {e}")
        return None

def get_user_comments(user, sort=None, limit=None, time=None, after=None):
    """Fetch user comments. Returns UserCommentsSent or None."""
    try:
        _ensure_initialized()
        task = _scraper_instance.ScrapUserComments(user, sort, limit, time, after)
        return task.Result
    except Exception as e:
        print(f"RedScraps Wrapper Error [get_user_comments]: {e}")
        return None