---
name: heathen-steamworks-unity-foundation
description: Orientation for an agent integrating raw Steamworks (User, Stats, Achievements, Leaderboards, App/DLC, client+listen-server dual-init) via Heathen's Foundation for Steamworks in Unity.
---

# Foundation for Steamworks (Unity)

A lightweight, modular integration layer over [Steamworks.NET](https://steamworks.github.io/) that
exposes Steam features through Unity components and a type-safe generated code wrapper
(`SteamTools.Game`). This is the open-source base layer — no paid features, Apache-2.0 licensed.

## What this product does

Foundation maps Steamworks.NET's raw SDK calls to Unity-friendly static facades and Inspector
components. It fully owns **User, Stats, Achievements, and Leaderboards** with a complete API +
components for each, plus data-plumbing (typed structs and events, no dedicated components yet)
for **Lobbies, Steam Input, Workshop, and Timeline**. As of v1.63.13 it also owns **client+listen-
server dual-init**: a single process can hold both a live Steam client session and a live Steam
Game Server session at once, each independently trackable and independently start/stoppable.

## Tier

**Foundation** — the base/open-source tier, standalone repo, no dependency on any paid Heathen
product. The paid **Toolkit for Steamworks** builds ergonomics (lobbies UI, friends, Steam Input
managers, inventory, UGC, overlay, server browser, etc.) on top of this. If you're working in a
project that has the Toolkit installed (`com.heathen.steamworks`), prefer its higher-level
`API.<Feature>` facades and components over calling this Foundation directly where both exist.

Toolkit lives in Heathen's private SourceRepo, not in this repo:
`SourceRepo/Unity/ToolkitSource/Assets/Toolkits/com.heathen.steamworks/` (that folder's own
`SKILL.md` documents the Toolkit side and cross-links back here). Toolkit-tier customers don't
have filesystem access to that path — it's sponsor-only — but its existence and relationship to
this Foundation is worth knowing regardless.

## Up

No local engine-level `SKILL.md` in this repo (it's standalone, not inside `SourceRepo`). See
`github.com/heathen-engineering/SourceRepo/SKILL.md` for the Heathen ecosystem overview if you
need it — the Unity-specific engine guide lives in that repo at `Unity/SKILL.md`.

## Key namespaces / entry points

Namespace: `Heathen.SteamworksIntegration` (data types, MonoBehaviours), `Heathen.SteamworksIntegration.API`
(static per-feature facades), `SteamTools` (generated wrapper: `Game`, `Interface`, `Events`).

| Area | Static facade | Notes |
| :--- | :--- | :--- |
| Client init/lifecycle | `SteamTools.Interface.Initialise()` / `.IsReady` | Also reachable no-code via the `InitializeSteamworks` component. |
| App/DLC (client) | `API.App.Client` | `Initialised`, `LoggedOn`, `Initialise(AppData, InputActionData[])`, DLC/subscription/beta queries. |
| App/DLC (listen server) | `API.App.Server` | See "Client+listen-server dual-init" below. |
| User | `API.User.Client` | `Id`, `Level`, `RichPresence`, `LoggedOn`, `AdvertiseGame(...)`. |
| Stats & Achievements | `API.StatsAndAchievements.Client` / `.Server` | `GetAchievement`, `ClearAchievement`, `StoreStats`, stat get/set; `.Server` variants take an explicit `CSteamID userId` (game-server-authoritative stat/achievement calls). |
| Leaderboards | `API.Leaderboards.Client` | `Find`, `FindOrCreate`, `DownloadEntries` (3 overloads), `UploadScore`, `GetEntryCount`. |
| Utilities | `API.Utilities.Client` / `.Server` | Ping, IP, VR/Big Picture/Steam Deck queries, virtual keyboard. |
| Generated wrapper | `SteamTools.Game` | Per-project typed access, e.g. `SteamTools.Game.Achievements.ACH_WIN_ONE_GAME.Unlock()`, `SteamTools.Game.Stats.NumWins.GetInt()` — generated via Project Settings ▸ Steamworks ▸ Generate Code. |

Data-plumbing-only structs (no dedicated ergonomic layer yet — Toolkit adds that): `LobbyData`,
`LobbyMemberData`, `InputActionData`/`InputActionSetData`/`InputActionStateData`,
`WorkshopItemEditorData`, `TimelineEventData`. `SteamTools.Events` exposes the matching raw Steam
callback events (lobby enter/leave/invite/chat, game-server connect/disconnect, etc.).

`UserData` wraps `CSteamID` with implicit conversions both ways — prefer it over touching
`CSteamID` directly.

### Client+listen-server dual-init (new in v1.63.13)

`API.App.Client` and `API.App.Server` now track fully independent initialisation state, so one
process can run a Steam client session and a Steam Game Server ("listen server", host-is-also-a-
player) session at the same time instead of the two being mutually exclusive at boot.

- `API.App.Server.Ready` — `bool`, true only when both `Initialised` and `LoggedOn`.
- `API.App.Server.OnReadyChanged` — `event Action<bool>`, fires on every `Ready` transition (connect,
  disconnect, connect failure, `LogOff`, `Shutdown`) — one subscription covers all of them.
- `API.App.Server.LogOff()` — cheap, keeps the native game-server context alive; use for stop/
  restart cycles (e.g. a listen server ending one hosted session and starting another).
- `API.App.Server.Shutdown()` — full teardown of the native context (also pumps
  `GameServer.RunCallbacks()` once after logging off, before tearing down, to avoid a stale-looking
  entry in the Server Browser).
- `API.App.Server.Initialise(AppData, SteamGameServerConfiguration)` — starts the server context.
  Enforces per-App-ID listen-server permission by reflectively checking the generated
  `SteamTools.Game.EnableListenServer` flag, even if called directly rather than through the
  generated wrapper.
- `SteamTools.Interface.IsReady` tracks client readiness only and is now correctly reset on
  shutdown (was a real bug fixed in this same release — previously stuck `true` after teardown).

Full detail: `com.heathen.steamworksfoundation/CHANGELOG.md` v1.63.13 entry.

## Dependencies

- **Steamworks.NET** (`com.rlabrecque.steamworks.net`) — third-party OSS, not written by Heathen.
  Guarded via UPM `versionDefines` (`STEAM_INSTALLED`) plus a `defineConstraints: ["!DISABLESTEAMWORKS"]`
  gate; the editor prompts to auto-install it on first import if missing. This package (Foundation
  for Steamworks) is the reference implementation of Heathen's non-UPM/third-party dependency-
  guarding pattern.
- **Heathen.GameFramework** (`com.heathen.gameframework`) — hard asmdef reference, auto-installs.
  Drives the `SteamworksSubsystem` (`[Subsystem(SubsystemScope.Global)]`) that pumps
  `SteamAPI.RunCallbacks()` every frame and whose Start Mode (`Automatic`/`OnDemand`/`Disabled`) is
  configured via Project Settings ▸ Subsystems ▸ Steamworks.
- Asmdef `Heathen.Foundations.Steamworks` also references `Unity.TextMeshPro`, `Unity.Localization`,
  `UniTask`, `Unity.Mathematics` — optional/guarded via their own `versionDefines`, not hard
  requirements for core Steam functionality.

## Common tasks

- **Initialise Steam (client)**: add `InitializeSteamworks` component to your first scene (no
  code), or call `SteamTools.Game.Initialise()` / `SteamTools.Interface.Initialise()` and subscribe
  to `SteamTools.Interface.OnReady`.
- **Start/stop a listen server independently of the client**: `API.App.Server.Initialise(appId,
  config)`, then `API.App.Server.LogOff()`/`.Shutdown()` to stop; subscribe to
  `API.App.Server.OnReadyChanged` to react to state changes. See the dual-init section above.
- **Unlock an achievement**: generated wrapper `SteamTools.Game.Achievements.<ACH_NAME>.Unlock()`,
  or `API.StatsAndAchievements.Client` directly by API name.
- **Read/write a stat and sync to Steam**: `SteamTools.Game.Stats.<StatName>.SetInt(...)` then
  `API.StatsAndAchievements.Client.StoreStats()`.
- **Query/set a user's stat from a listen server**: `API.StatsAndAchievements.Server.GetUserStat(
  userId, statApiName, out data)` (and the matching `Set*` calls) — server-authoritative, takes an
  explicit `CSteamID`.
- **Find and upload to a leaderboard**: `API.Leaderboards.Client.FindOrCreate(name, sortMethod,
  displayType, callback)` then `.UploadScore(leaderboard, method, score, ...)`.
- **Convert between `CSteamID` and Heathen's wrapper**: `UserData.Get(rawId)` / implicit
  `UserData ↔ CSteamID`/`ulong` conversions.
- **Generate the typed per-project wrapper**: Project Settings ▸ Steamworks ▸ define App IDs/API
  names ▸ Generate Code → `Assets/Scripts/Generated/SteamTools.Game.cs`.

Signatures verified directly against `com.heathen.steamworksfoundation/Runtime/API/*.cs` and
`Runtime/SteamTools.cs` in this repo, not against design docs — this repo has no `DesignSpecs/`
directory of unbuilt-feature specs to cross-check against, unlike some sibling repos.

## Docs

KB root: `https://heathen.group/kb/foundation-steamworks/` (from `package.json`'s
`documentationUrl`). Per-feature articles likely follow the KB's usual
`https://heathen.group/kb/<feature>/` pattern — check the live KB rather than guessing a specific
slug.

## Version

`com.heathen.steamworksfoundation/package.json` (`version` field) + `com.heathen.steamworksfoundation/CHANGELOG.md`.
Current: **v1.63.13** (2026-07-20) — added client+listen-server dual-init
(`API.App.Server.Ready`/`OnReadyChanged`/`LogOff`/`Shutdown`), fixed a `SteamTools.Interface.IsReady`
reset bug, fixed a Server Browser stale-registration edge case on shutdown, and fixed a Start Mode
scripting-define reflection bug in the code generator. See the CHANGELOG for full detail including
same-day regression fixes.
