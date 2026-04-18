# Foundation for Steamworks

![License](https://img.shields.io/badge/License-Apache_2.0-blue?style=flat-square)
![Maintained](https://img.shields.io/badge/Maintained%3F-yes-green?style=flat-square)
![Unity](https://img.shields.io/badge/Unity-6%20%2B-black?style=flat-square&logo=unity&logoColor=white)
[![Dependency](https://img.shields.io/badge/Dependency-Steamworks.NET-lightgrey?style=flat-square)](https://steamworks.github.io/)

A lightweight, modular integration layer for [Steamworks.NET](https://steamworks.github.io/) that exposes Steam features through ScriptableObject-based components and a type-safe generated code wrapper.

-----

## 🛠 Also Available For

[![O3DE](https://img.shields.io/badge/O3DE-25.10%20%2B-%2300AEEF?style=for-the-badge&logo=data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCAyNCAyNCI+PHBhdGggZmlsbD0id2hpdGUiIGQ9Ik0xMiAxTDEgNy40djkuMkwxMiAyM2wxMS02LjRWNy40TDEyIDF6bTkuMSAxNC45TDExLjUgMjEuM2wtOC42LTYuNFY4LjFsOC42LTYuNCA5LjEgNi40djYuOHpNMTEuNSA0LjZMMi45IDkuNnY0LjhsOC42IDUuMSA4LjYtNS4xVjkuNmwtOC42LTUuMHoiLz48L3N2Zz4=)](https://github.com/heathen-engineering/O3DE-Foundation-for-Steamworks)

-----

## Become a GitHub Sponsor
[![Discord](https://img.shields.io/badge/Discord--1877F2?style=social&logo=discord)](https://discord.gg/6X3xrRc)
[![GitHub followers](https://img.shields.io/github/followers/heathen-engineering?style=social)](https://github.com/heathen-engineering?tab=followers)  
Support Heathen by becoming a [GitHub Sponsor](https://github.com/sponsors/heathen-engineering). Sponsorship directly funds the development and maintenance of free tools like this, as well as our game development [Knowledge Base](https://heathen.group/) and community on [Discord](https://discord.gg/6X3xrRc).

Sponsors also get access to our private SourceRepo, which includes developer tools for O3DE, Unreal, Unity, and Godot.  
Learn more or explore other ways to support @ [heathen.group/kb](https://heathen.group/kb/do-more/)

-----

## What it does

Foundation maps Steamworks interfaces to Unity-friendly patterns, primarily focused on **User Data, Stats, Achievements, and Leaderboards**. It operates via two main systems:

| System | Purpose |
|-----|---------|
| **Generated Wrapper** | A static `SteamTools.Game` class that provides type-safe access to your specific App IDs and API names. |
| **Modular Components** | MonoBehaviours that allow you to build Steam-driven UI in the Inspector without writing glue code. |

The following core features are covered:

- **Core** — Application IDs, multi-app support (Main, Demo, Playtest), and automated initialization.
- **User** — Persona names, Steam levels, online status, and avatars.
- **Stats** — Integer and Float stat management with local caching and server sync.
- **Achievements** — Localized names, descriptions, icons, and unlock/lock state.
- **Leaderboards** — Score uploading, rank retrieval, and entry display.

---

## Requirements

- Unity engine **6.0** or compatible
- A registered [Steamworks developer account](https://partner.steamgames.com/) 
- [Steamworks.NET](https://steamworks.github.io/)

-----

## Installation

### Via Unity Package Manager (UPM)

1.  In Unity, go to `Window > Package Manager`.
2.  Click **+** \> **Add package from git URL**.
3.  Enter: `https://github.com/heathen-engineering/Unity-Foundation-for-Steamworks.git?path=/com.heathen.steamworksfoundation`

**Steamworks.NET** is installed automatically. On the first domain reload after Foundation is imported, the editor will detect if Steamworks.NET is missing and prompt you to install the recommended version. If you need a specific version, menu items are available under `Help > Heathen > Steamworks Foundation > Install Steamworks.NET …`.

-----

## Setup & Workflow

### 1\. Configuration

Open **Project Settings \> Steamworks**. Define your App IDs and list your API names (Achievements, Stats, Leaderboards) exactly as they appear in the Steamworks Partner Portal.

### 2\. Code Generation

Click **Generate Code** in the settings panel. This creates `Assets/Scripts/Generated/SteamTools.Game.cs`. This wrapper provides static access to your data:

```csharp
// Type-safe access to your Steam data
uint id = SteamTools.Game.AppId;
SteamTools.Game.Achievements.ACH_WIN_ONE_GAME.Unlock();
int wins = SteamTools.Game.Stats.NumWins.GetInt();
```

### 3\. Initialisation

**No code required:** Add the `InitializeSteamworks` component to a GameObject in your startup scene. It calls `SteamTools.Game.Initialise()` automatically and respects the `OnReady` event without any scripting.

**From code:** Call the init method once at startup and subscribe to `OnReady` to know when leaderboard handles and user data are fully populated.

```csharp
void Awake() {
    SteamTools.Game.Initialise();
    SteamTools.Interface.OnReady += () => Debug.Log("Steam is Ready");
}
```

-----

## Component Reference

Foundation uses a **Parent-Child** modular design. The parent component holds the data reference, and child components on the same GameObject automatically react to it.

### User & Achievements

| Parent | Child Component | Purpose |
|--------|-----------------|---------|
| **SteamUserData** | `SteamUserName` | Displays persona name in a TMP label. |
| | `SteamUserStatus` | Displays online/offline status. |
| **SteamAchievementData** | `SteamAchievementIcon` | Displays achievement icon in a UI Image. |
| | `SteamAchievementChanged` | Fires a UnityEvent on unlock. |

### Stats & Leaderboards

| Parent | Child Component | Purpose |
|--------|-----------------|---------|
| **SteamLeaderboardData** | `SteamLeaderboardRank` | Displays the local user's rank. |
| | `SteamLeaderboardDisplay` | Populates a list/grid of leaderboard entries. |

-----

## Usage Overview

**C\# Example — Committing Stat Changes:**

```csharp
using Heathen.SteamworksIntegration;

// Update local cache
SteamTools.Game.Stats.NumWins.SetInt(newWins);

// Sync with Steam Servers
API.StatsAndAchievements.Client.StoreStats();
```

**C\# Example — Uploading Leaderboard Scores:**

```csharp
var board = SteamTools.Game.Leaderboards.TopScores;
board.UploadScore(score, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, 
    (result, error) => { /* handle result */ });
```

-----

## Public Namespaces & Headers

| Namespace | Contents |
|-----------|----------|
| `Heathen.SteamworksIntegration` | Core data types (`UserData`, `AchievementData`) and components. |
| `Heathen.SteamworksIntegration.API` | Low-level static wrappers for Steam interfaces. |
| `SteamTools` | The generated wrapper (`Game`, `Interface`, `Events`). |

