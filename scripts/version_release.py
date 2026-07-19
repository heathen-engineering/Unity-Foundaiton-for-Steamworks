#!/usr/bin/env python3
"""
Detects a version bump in this repo's single package.json between two git refs. On a bump:
tags the commit (v<version>), appends the package's CHANGELOG.md with the real commit subjects
since the previous tag, zips the package folder, and creates a GitHub Release on that tag
(marked prerelease for any 0.x version). No build/compile step -- this repo ships source directly,
so "packaging" just means archiving the package folder as-is.

Mirrors the per-product scheme documented in the ToolkitSource monorepo's docs/versioning.md,
simplified for a single-package repo (no per-product loop, no engine dispatch). Intended to run
in CI on every push to main; see .github/workflows/version-release.yml.
"""
import json
import re
import subprocess
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent


def sh(*args, **kwargs):
    try:
        return subprocess.run(args, cwd=REPO_ROOT, capture_output=True, text=True, check=True, **kwargs).stdout
    except subprocess.CalledProcessError as e:
        print(f"Command failed: {' '.join(args)}\nstdout: {e.stdout}\nstderr: {e.stderr}", file=sys.stderr)
        raise


def find_package_json() -> Path:
    candidates = [p for p in REPO_ROOT.glob("*/package.json")]
    if len(candidates) != 1:
        print(f"Expected exactly one top-level */package.json, found {len(candidates)}: {candidates}",
              file=sys.stderr)
        sys.exit(1)
    return candidates[0]


def read_json_at(ref: str, path: Path):
    rel = path.relative_to(REPO_ROOT).as_posix()
    try:
        text = sh("git", "show", f"{ref}:{rel}")
    except subprocess.CalledProcessError:
        return None
    try:
        return json.loads(text)
    except json.JSONDecodeError:
        return None


def version_tuple(v: str):
    parts = re.findall(r"\d+", v)
    return tuple(int(p) for p in parts) if parts else (v,)


def version_increased(old, new) -> bool:
    if old is None:
        return True
    if old == new:
        return False
    try:
        return version_tuple(new) > version_tuple(old)
    except TypeError:
        return new != old


def is_prerelease(version: str) -> bool:
    major = re.match(r"(\d+)", version)
    return bool(major) and int(major.group(1)) == 0


def latest_tag() -> str | None:
    out = sh("git", "tag", "-l", "v*", "--sort=-v:refname").strip().splitlines()
    return out[0] if out else None


def commit_subjects_since(prev_tag: str | None, package_dir: Path) -> list[str]:
    rel = package_dir.relative_to(REPO_ROOT).as_posix()
    rev_range = f"{prev_tag}..HEAD" if prev_tag else "HEAD"
    out = sh("git", "log", rev_range, "--no-merges", "--format=%s", "--", rel)
    subjects = [line.strip() for line in out.splitlines() if line.strip()]
    return [s for s in subjects if "[skip ci]" not in s]


def append_changelog(package_dir: Path, display: str, version: str, subjects: list[str]):
    changelog = package_dir / "CHANGELOG.md"
    title = f"# {display} — Changelog\n\n"
    date = sh("date", "+%Y-%m-%d").strip()
    if subjects:
        body = "\n".join(f"- {s}" for s in subjects) + "\n\n"
    else:
        body = "(auto-recorded version bump; no distinct commits found for this range)\n\n"
    entry = f"## v{version} — {date}\n\n{body}"
    if changelog.exists():
        existing = changelog.read_text()
        parts = existing.split("\n\n", 1)
        rest = parts[1] if len(parts) > 1 and parts[0].startswith("#") else existing
        changelog.write_text(title + entry + rest)
    else:
        changelog.write_text(title + entry)


def create_tag(tag: str, message: str):
    sh("git", "tag", "-a", tag, "-m", message)
    sh("git", "push", "origin", tag)


def package_and_release(package_dir: Path, tag: str, display: str, version: str, subjects: list[str]):
    rel = package_dir.relative_to(REPO_ROOT).as_posix()
    zip_path = REPO_ROOT / f"{tag}.zip"
    sh("git", "archive", "--format=zip", "-o", str(zip_path), "HEAD", "--", rel)
    title = f"{display} {tag}"
    notes = "\n".join(f"- {s}" for s in subjects) if subjects else "(no distinct commits for this range)"
    notes = f"{notes}\n\nSee `{rel}/CHANGELOG.md`."
    args = ["gh", "release", "create", tag, str(zip_path), "--title", title, "--notes", notes]
    if is_prerelease(version):
        args.append("--prerelease")
    sh(*args)
    zip_path.unlink(missing_ok=True)


def main():
    before, after = sys.argv[1], sys.argv[2]
    package_path = find_package_json()
    package_dir = package_path.parent

    old_manifest = read_json_at(before, package_path)
    new_manifest = read_json_at(after, package_path)
    if new_manifest is None:
        print("No package.json at HEAD; nothing to do.")
        return

    old_version = old_manifest.get("version") if old_manifest else None
    new_version = new_manifest.get("version")
    if not version_increased(old_version, new_version):
        print(f"No version bump ({old_version} -> {new_version}); nothing to do.")
        return

    display = new_manifest.get("displayName") or new_manifest.get("name")
    tag = f"v{new_version}"
    prev_tag = latest_tag()
    subjects = commit_subjects_since(prev_tag, package_dir)

    print(f"Version bump detected: {old_version} -> {new_version}, tagging {tag}")
    create_tag(tag, f"{display} {new_version}")
    append_changelog(package_dir, display, new_version, subjects)
    package_and_release(package_dir, tag, display, new_version, subjects)
    print(f"Released {tag}.")


if __name__ == "__main__":
    main()
