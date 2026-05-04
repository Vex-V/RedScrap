"""
Builds all platform wheels.

Usage:
    python build_all.py

Produces:
    dist/redscrapslib-<ver>-cp313-cp313-win_amd64.whl
    dist/redscrapslib-<ver>-py3-none-macosx_12_0_universal2.whl
    dist/redscrapslib-<ver>-py3-none-manylinux2014_x86_64.whl
"""

import glob
import subprocess
import sys
import os

MAC_PLATFORMS = [
    "macosx_12_0_universal2",       # Apple Silicon + Intel
]

LINUX_PLATFORMS = [
    "manylinux2014_x86_64",         # x86_64 Linux (WSL2, Ubuntu, etc.)
]


def run(*args):
    subprocess.run(args, check=True)


def build_windows_wheel() -> str:
    print("==> Building Windows wheel...")
    run(sys.executable, "-m", "build", "--wheel")
    wheels = sorted(glob.glob("dist/*-win_amd64.whl"))
    if not wheels:
        raise FileNotFoundError("Windows wheel not found in dist/ after build.")
    return wheels[-1]


def retag(wheel_path: str, platform: str) -> str:
    result = subprocess.run(
        [
            sys.executable, "-m", "wheel", "tags",
            "--python-tag",   "py3",
            "--abi-tag",      "none",
            "--platform-tag", platform,
            wheel_path,
        ],
        check=True,
        capture_output=True,
        text=True,
    )
    output = result.stdout.strip()
    return output.split()[-1] if output else ""


def build_mac_wheels(base_wheel: str):
    print("\n==> Building macOS wheels...")
    for plat in MAC_PLATFORMS:
        out = retag(base_wheel, plat)
        print(f"  Created: {out or plat}")


def build_linux_wheels(base_wheel: str):
    print("\n==> Building Linux wheels...")
    for plat in LINUX_PLATFORMS:
        out = retag(base_wheel, plat)
        print(f"  Created: {out or plat}")


def main():
    os.makedirs("dist", exist_ok=True)

    win_wheel = build_windows_wheel()
    print(f"  Created: {win_wheel}")

    build_mac_wheels(win_wheel)
    build_linux_wheels(win_wheel)

    print("\n==> All wheels in dist/:")
    for w in sorted(glob.glob("dist/*.whl")):
        print(f"  {w}")


if __name__ == "__main__":
    main()
