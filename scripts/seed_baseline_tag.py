#!/usr/bin/env python3
"""
One-time pass: tag this repo's CURRENT package.json version if it has never been tagged, so
version_release.py's HEAD~1..HEAD diffing has a real starting point instead of leaving every
already-shipped version untagged. Safe to re-run -- skips if the tag already exists (local or
remote). Run this once, by hand, before the version-release workflow goes live.
"""
import subprocess
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from version_release import find_package_json, read_json_at, append_changelog, create_tag, \
    package_and_release, REPO_ROOT


def tag_exists(tag: str) -> bool:
    local = subprocess.run(["git", "tag", "-l", tag], cwd=REPO_ROOT, capture_output=True, text=True).stdout.strip()
    if local:
        return True
    remote = subprocess.run(["git", "ls-remote", "--tags", "origin", tag], cwd=REPO_ROOT,
                             capture_output=True, text=True).stdout.strip()
    return bool(remote)


def main():
    pkg = find_package_json()
    manifest = read_json_at("HEAD", pkg)
    version = manifest.get("version")
    display = manifest.get("displayName") or manifest.get("name")
    tag = f"v{version}"
    if tag_exists(tag):
        print(f"Already tagged, skipping: {tag}")
        return
    print(f"Seeding baseline tag: {tag}")
    create_tag(tag, f"{display} {version} (baseline)")
    append_changelog(pkg.parent, display, version, ["(baseline release; prior history not itemized)"])
    package_and_release(pkg.parent, tag, display, version, [])
    print(f"Seeded {tag}.")


if __name__ == "__main__":
    main()
