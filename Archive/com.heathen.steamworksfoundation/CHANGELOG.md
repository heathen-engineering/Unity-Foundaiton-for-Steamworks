# Foundation for Steamworks — Changelog

## v1.63.13 — 2026-07-20

- **New**: Steam listen-server support — `API.App.Client`/`API.App.Server` now track independent
  initialisation state, so a single process can bring up the Steam client and a Steam Game Server
  context together (a "listen server" whose host is also a player), instead of the two being
  mutually exclusive at boot.
  - `Server.Ready`/`Server.OnReadyChanged(bool)` — one event covering both becoming ready and no
    longer ready (connect, disconnect, connect failure, `LogOff`, `Shutdown`).
  - `Server.LogOff()` (cheap, keeps the native context) and `Server.Shutdown()` (full teardown) as
    explicit, independent, repeatable lifecycle calls — supports on-demand start/stop/restart, e.g. a
    listen server ending one hosted session and starting another.
  - `PumpCallbacks()` and the whole-app `Shutdown()` now drive whichever context(s) are actually
    live, rather than a compile-time `UNITY_SERVER` either/or.
  - Central per-App-ID permission enforcement in `API.App.Server.Initialise` (see Toolkit's
    changelog for the authoring side) — reflectively reads the generated
    `SteamTools.Game.EnableListenServer` flag and refuses to initialise if the app hasn't been
    granted listen-server permission, even when called directly rather than through the generated
    3-way entry point.
- **Fix**: `Server.Shutdown()` now pumps `GameServer.RunCallbacks()` once after logging off and
  before tearing down the native context, as a defensive measure against a Steam Game Server
  registration being left looking "live" in the Server Browser after an otherwise-clean stop.
- **Fix**: `SteamworksSubsystem.ResolveStartMode()`'s reflection lookup for the generated
  `SteamTools.Game.StartMode` property silently never worked for any configured Start Mode
  (including `Disabled`), because `HEATHEN_GAMEFRAMEWORK` was only ever an asmdef `versionDefines`
  entry — scoped to this package's own assemblies, never reaching the implicit `Assembly-CSharp`
  assembly the generated wrapper actually compiles into. The code generator now also pushes
  `HEATHEN_GAMEFRAMEWORK` as a real Player Settings scripting define (mirroring how the existing
  `APP{id}` defines already work), so the baked `StartMode` property actually compiles and resolves
  correctly. Takes effect on the next Generate Code.
- **Fix**: `SteamTools.Interface.IsReady` was set `true` in `RaiseOnReady` but never reset — any code
  reading it as a live "is Steam still usable" guard (e.g. `SteamInputManager.LateUpdate` before
  calling `SteamInput.RunFrame`) kept seeing `true` after a real shutdown and crashed calling into a
  torn-down `SteamAPI` (`InvalidOperationException: Steamworks is not initialized`, hit on Stop Play
  Mode during real testing). Now reset via a shutdown handler alongside the client's own teardown.
- **Fix (regression from this same release, caught in same-day testing)**: the new Editor
  `playModeStateChanged` shutdown hook was calling the whole-app `Shutdown()` (tearing down the
  client too) at `ExitingPlayMode`, while the frame loop is still running — anything still ticking
  that frame (same `SteamInputManager.LateUpdate` case above) then called into an already-dead
  client. Narrowed to call `Server.Shutdown()` only; the client continues to shut down at its normal,
  later time via `Application.quitting`.

## v1.63.12 — 2026-07-19

- (baseline release; prior history not itemized)

