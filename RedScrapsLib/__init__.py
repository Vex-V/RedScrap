import os
import sys
from pythonnet import load


try:
    load("coreclr")
except Exception:
    
    pass

import clr


current_dir = os.path.dirname(os.path.abspath(__file__))
dll_name = "RedScrap.dll"
dll_path = os.path.join(current_dir, dll_name)

if current_dir not in sys.path:
    sys.path.append(current_dir)

if not os.path.exists(dll_path):
    raise FileNotFoundError(f"Missing DLL: {dll_path}\n"
                            "Ensure the DLL and its .deps.json are in the same folder.")


clr.AddReference(dll_path)

try:

    from RedScraps import Scrappers
except ImportError as e:
    raise ImportError(f"Could not find namespace 'RedScraps' in {dll_name}. "
                     "Check your C# code's 'public' modifiers and namespace string.") from e


def get_home(subreddit, sort="hot", limit=100, time=None, after=None):
    """
    Fetches the home page posts for a subreddit.
    Returns a C# HomeSent object or None if it fails.
    """
    try:

        task = Scrappers.ScrapHome(subreddit, sort, limit, time, after)
        return task.Result # Blocks until the C# Task completes
    except Exception as e:
        print(f"RedScraps Wrapper Error [get_home]: {e}")
        return None

def get_comments(subreddit, post_id, sort="confidence", limit=100):
    """
    Fetches the flattened comment tree for a specific post.
    Returns a C# CommentSent object or None if it fails.
    """
    try:
        task = Scrappers.ScrapComments(subreddit, post_id, sort, limit)
        return task.Result
    except Exception as e:
        print(f"RedScraps Wrapper Error [get_comments]: {e}")
        return None