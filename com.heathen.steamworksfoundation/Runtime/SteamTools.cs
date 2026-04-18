// Copyright 2024 Heathen Engineering
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if !DISABLESTEAMWORKS && STEAM_INSTALLED
using Heathen.SteamworksIntegration;
using Heathen.SteamworksIntegration.API;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if UNITASK_INSTALLED
using Cysharp.Threading.Tasks;
#endif
using UnityEngine;
// ReSharper disable NotAccessedField.Local

namespace SteamTools
{
    // -----------------------------------------------------------------------
    // Delegate definitions (Foundation-compatible types only)
    // -----------------------------------------------------------------------

    /// <summary>A delegate carrying a single string.</summary>
    public delegate void StringDelegate(string message);
    /// <summary>A delegate carrying a single bool.</summary>
    public delegate void BoolDelegate(bool value);
    /// <summary>A delegate carrying a single <see cref="EResult"/>.</summary>
    public delegate void EResultDelegate(EResult result);
    /// <summary>Invoked when a Steam server connection attempt fails.</summary>
    public delegate void SteamServerConnectFailureDelegate(EResult result, bool retrying);
    /// <summary>Invoked when a DLC is installed.</summary>
    public delegate void AppDataDelegate(AppData app);
    /// <summary>Invoked when Steam text input is dismissed.</summary>
    public delegate void SteamTextInputDelegate(bool submitted, string textValue);
    /// <summary>Invoked when a friend chat message arrives.</summary>
    public delegate void SteamFriendChatMsgDelegate(UserData user, string message, EChatEntryType type);
    /// <summary>Invoked when a friend's rich presence changes.</summary>
    public delegate void SteamFriendRichPresenceUpdateDelegate(UserData user, AppData app);
    /// <summary>Invoked when a user's persona state changes.</summary>
    public delegate void PersonaStateChangeEvent(UserData user, EPersonaChange changeFlag);
    /// <summary>Invoked when a Steam Input controller connects or disconnects.</summary>
    public delegate void SteamInputHandleDelegate(InputHandle_t handle);
    /// <summary>Invoked when a Steam inventory result is ready.</summary>
    public delegate void SteamInventoryResultReadyDelegate(SteamInventoryResult_t result);
    /// <summary>Invoked when a microtransaction authorisation response arrives.</summary>
    public delegate void SteamMtxTranAuthDelegate(AppData app, ulong orderId, bool authorised);
    /// <summary>Invoked when a lobby is successfully entered.</summary>
    public delegate void SteamLobbyEnterSuccessDelegate(LobbyData lobby);
    /// <summary>Invoked when entering a lobby fails.</summary>
    public delegate void SteamLobbyEnterFailedDelegate(LobbyData lobby, EChatRoomEnterResponse result);
    /// <summary>Invoked when lobby data or a lobby member's data is updated.</summary>
    /// <remarks><paramref name="member"/> equals <paramref name="lobby"/> when the lobby's own data changed.</remarks>
    public delegate void SteamLobbyDataUpdateDelegate(LobbyData lobby, UserData member);
    /// <summary>Invoked when a raw chat message arrives in a lobby.</summary>
    public delegate void SteamLobbyChatMsgDelegate(LobbyData lobby, UserData sender, byte[] data, EChatEntryType type);
    /// <summary>Invoked for single-lobby events (enter or leave).</summary>
    public delegate void SteamLobbyDataDelegate(LobbyData lobby);
    /// <summary>Invoked when the chat membership of a lobby changes.</summary>
    public delegate void SteamLobbyChatUpdateDelegate(LobbyData lobby, UserData user, EChatMemberStateChange changes);
    /// <summary>Invoked when a lobby game server is created.</summary>
    public delegate void SteamLobbyGameServerDelegate(LobbyData lobby, CSteamID server, string ip, ushort port);
    /// <summary>Invoked when a lobby invitation arrives.</summary>
    public delegate void SteamLobbyInviteDelegate(UserData fromUser, LobbyData forLobby, GameData inGame);
    /// <summary>Invoked when the Steam favourites list changes.</summary>
    public delegate void SteamFavouritesListChangeDelegate(string ip, ushort queryPort, ushort connPort, AppData app, uint flags, bool add, uint accountId);
    /// <summary>Invoked when the user requests to join a lobby from the Steam UI.</summary>
    public delegate void SteamLobbyJoinRequestDelegate(LobbyData lobby, UserData user);
    /// <summary>Invoked when the game overlay is activated or deactivated.</summary>
    public delegate void SteamGameOverlayActivatedDelegate(bool active);
    /// <summary>Invoked when a game server change is requested.</summary>
    public delegate void SteamGameServerChangeRequestedDelegate(string server, string password);
    /// <summary>Invoked when a rich presence join request arrives.</summary>
    public delegate void SteamRichPresenceJoinRequestedDelegate(UserData user, string connectionString);
    /// <summary>Invoked when a party beacon reservation notification arrives.</summary>
    public delegate void SteamReservationNotificationDelegate(UserData user, PartyBeaconID_t party);
    /// <summary>Invoked for remote play session connect/disconnect events.</summary>
    public delegate void SteamRemotePlaySessionIdDelegate(RemotePlaySessionID_t sessionId);
    /// <summary>Invoked when a screenshot is ready.</summary>
    public delegate void SteamScreenshotReadyDelegate(ScreenshotHandle screenshotHandle, EResult result);
    /// <summary>Invoked when stats are received for a user.</summary>
    public delegate void SteamStatsReceivedDelegate(GameData game, EResult result, UserData user);
    /// <summary>Invoked when stats are stored.</summary>
    public delegate void SteamStatsStoredDelegate(GameData game, EResult result);
    /// <summary>Invoked when a user's stats are unloaded.</summary>
    public delegate void SteamUserStatsUnloadedDelegate(UserData user);
    /// <summary>Invoked when an achievement is stored for a user.</summary>
    public delegate void SteamUserAchievementStoredDelegate(UserAchievementStoredData data);

    // -----------------------------------------------------------------------
    // Interface
    // -----------------------------------------------------------------------

    /// <summary>
    /// Provides a static interface for interacting with Steam tools and utilities.
    /// </summary>
    public static class Interface
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            _initalised     = false;
            IsReady         = false;
            _boards         = new();
            _sets           = new();
            _actions        = new();
            _whenReadyCalls = new();
        }

        private static bool _initalised;

        /// <summary>
        /// A property indicating whether the Steam integration has been successfully initialised.
        /// </summary>
        public static bool IsInitialised => App.Initialised;

        /// <summary>
        /// Controls whether debugging mode is enabled for the Steam integration system.
        /// </summary>
        public static bool IsDebugging
        {
            get => App.IsDebugging;
            set => App.IsDebugging = value;
        }

        /// <summary>
        /// A property indicating whether the SteamTools interface is fully initialised and ready for use.
        /// </summary>
        public static bool IsReady { get; private set; }

        /// <summary>
        /// An event triggered when the Steam integration process has successfully completed its initialisation.
        /// </summary>
        public static event Action OnReady;

        /// <summary>
        /// An event triggered when an error occurs during the Steam integration initialisation process.
        /// </summary>
        public static event Action<string> OnInitialisationError;

        private static Dictionary<string, LeaderboardData>    _boards         = new();
        private static Dictionary<string, InputActionSetData> _sets           = new();
        private static Dictionary<string, InputActionData>    _actions        = new();
        private static List<Action>                            _whenReadyCalls = new();

        /// <summary>
        /// Initialises the SteamTools interface by locating and invoking
        /// <c>SteamTools.Game.Initialise()</c> via reflection.
        /// </summary>
        public static void Initialise()
        {
            if (_initalised) return;

            _initalised = true;

            Events.Initialise();
            Events.OnSteamInitialisationError += HandleInitialisedError;
            try
            {
                Type gameType = null;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    gameType = asm.GetType("SteamTools.Game");
                    if (gameType == null) continue;
                    Debug.Log($"Found SteamTools.Game in assembly: {asm.GetName().Name}");
                    break;
                }

                if (gameType != null)
                {
                    var initMethod = gameType.GetMethod("Initialise", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (initMethod != null)
                        initMethod.Invoke(null, null);
                    else
                        Debug.LogError("Unable to locate SteamTools.Game.Initialise method make sure your generate wrapper before attempting to initialise.");
                }
                else
                {
                    Debug.LogError("Unable to locate SteamTools.Game class make sure your generate wrapper before attempting to initialise.");
                    OnInitialisationError?.Invoke("Unable to locate SteamTools.Game class");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reflecting Game.AppId: {e.Message}");
                OnInitialisationError?.Invoke($"Error reflecting Game.AppId: {e.Message}");
            }
        }

        /// <summary>
        /// Registers a callback to be executed when the SteamTools interface is ready for use.
        /// If the interface is already ready, the callback is invoked immediately.
        /// </summary>
        public static void WhenReady(Action callback)
        {
            if (callback == null)
                return;

            if (IsReady)
                callback.Invoke();
            else
                _whenReadyCalls.Add(callback);
        }

        private static void HandleInitialisedError(string arg0)
        {
            OnInitialisationError?.Invoke(arg0);
        }

        /// <summary>
        /// Marks the SteamTools interface as ready and triggers the <see cref="OnReady"/> event.
        /// Assigns the leaderboard, input action set, and input action maps, then processes
        /// any pending callbacks registered with <see cref="WhenReady(Action)"/>.
        /// </summary>
        public static void RaiseOnReady(Dictionary<string, LeaderboardData> boardMap,
                                        Dictionary<string, InputActionSetData> setMap,
                                        Dictionary<string, InputActionData> actionMap)
        {
            _boards  = boardMap;
            _sets    = setMap;
            _actions = actionMap;
            IsReady  = true;
            OnReady?.Invoke();
            foreach (var call in _whenReadyCalls)
            {
                try
                {
                    call?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            _whenReadyCalls.Clear();
        }

        /// <summary>
        /// Adds a new leaderboard to the internal collection.
        /// Safe to call multiple times with the same key.
        /// </summary>
        public static void AddBoard(LeaderboardData board)
        {
            _boards.TryAdd(board.apiName, board);
        }

        /// <summary>Returns the <see cref="LeaderboardData"/> for the given API name, or default.</summary>
        public static LeaderboardData GetBoard(string name) => _boards.GetValueOrDefault(name);

        /// <summary>Returns all registered leaderboards.</summary>
        public static LeaderboardData[] GetBoards() => _boards.Values.ToArray();

        /// <summary>Returns the <see cref="InputActionSetData"/> for the given name, or default.</summary>
        public static InputActionSetData GetSet(string name) => _sets.GetValueOrDefault(name);

        /// <summary>Returns the <see cref="InputActionData"/> for the given action name, or default.</summary>
        public static InputActionData GetAction(string name) => _actions.GetValueOrDefault(name);
    }

    // -----------------------------------------------------------------------
    // Events
    // -----------------------------------------------------------------------

    /// <summary>
    /// Exposes every Steam callback as a static C# event.
    /// Foundation owns this class; Toolkit subscribes to and re-publishes
    /// the events it enriches with Toolkit-only types.
    /// </summary>
    public static class Events
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            _initialised = false;

            OnSteamInitialised                 = null;
            OnSteamInitialisationError         = null;
            OnDlcInstalled                     = null;
            OnNewUrlLaunchParameters           = null;
            OnSteamServersConnected            = null;
            OnSteamServerConnectFailure        = null;
            OnSteamServersDisconnected         = null;
            OnGamepadTextInputShown            = null;
            OnGamepadTextInputDismissed        = null;
            OnGameConnectedFriendChatMsg       = null;
            OnFriendRichPresenceUpdate         = null;
            OnPersonaStateChange               = null;
            OnControllerConnected              = null;
            OnControllerDisconnected           = null;
            OnInventoryDefinitionUpdate        = null;
            OnInventoryResultReady             = null;
            OnMicroTxnAuthorisationResponse    = null;
            OnLobbyEnterSuccess                = null;
            OnLobbyEnterFailed                 = null;
            OnLobbyDataUpdate                  = null;
            OnLobbyChatMsg                     = null;
            OnAskedToLeaveLobby                = null;
            OnLobbyChatUpdate                  = null;
            OnLobbyGameServer                  = null;
            OnLobbyLeave                       = null;
            OnLobbyInvite                      = null;
            OnFavouritesListChanged             = null;
            OnLobbyJoinRequested               = null;
            OnGameOverlayActivated             = null;
            OnGameServerChangeRequested        = null;
            OnRichPresenceJoinRequested        = null;
            OnReservationNotification          = null;
            OnActiveBeaconsUpdated             = null;
            OnAvailableBeaconLocationsUpdated  = null;
            OnRemotePlaySessionConnected       = null;
            OnRemotePlaySessionDisconnected    = null;
            OnRemoteStorageLocalFileChange     = null;
            OnScreenshotReady                  = null;
            OnScreenshotRequested              = null;
            OnStatsReceived                    = null;
            OnStatsUnloaded                    = null;
            OnUserAchievementStored            = null;
            OnStatsStored                      = null;
            OnAppResumeFromSuspend             = null;
            OnKeyboardShown                    = null;
            OnKeyboardClosed                   = null;

            DisposeCallbacks();
        }

        private static bool _initialised;

        // -----------------------------------------------------------------------
        // Event declarations
        // -----------------------------------------------------------------------

        /// <summary>Fired when Steam has been successfully initialised.</summary>
        public static event Action             OnSteamInitialised;
        /// <summary>Fired when Steam initialisation fails. Carries the error message.</summary>
        public static event StringDelegate     OnSteamInitialisationError;
        /// <summary>Fired when a DLC is installed.</summary>
        public static event AppDataDelegate    OnDlcInstalled;
        /// <summary>Fired when new URL launch parameters are detected.</summary>
        public static event Action             OnNewUrlLaunchParameters;
        /// <summary>Fired when a connection to Steam servers is established.</summary>
        public static event Action             OnSteamServersConnected;
        /// <summary>Fired when a connection to Steam servers fails.</summary>
        public static event SteamServerConnectFailureDelegate OnSteamServerConnectFailure;
        /// <summary>Fired when the client loses its Steam server connection.</summary>
        public static event EResultDelegate    OnSteamServersDisconnected;
        /// <summary>Fired when the gamepad text input UI is shown.</summary>
        public static event Action             OnGamepadTextInputShown;
        /// <summary>Fired when the gamepad text input UI is dismissed.</summary>
        public static event SteamTextInputDelegate OnGamepadTextInputDismissed;
        /// <summary>Fired when a friend chat message arrives.</summary>
        public static event SteamFriendChatMsgDelegate OnGameConnectedFriendChatMsg;
        /// <summary>Fired when a friend's rich presence data changes.</summary>
        public static event SteamFriendRichPresenceUpdateDelegate OnFriendRichPresenceUpdate;
        /// <summary>Fired when a user's persona state changes.</summary>
        public static event PersonaStateChangeEvent    OnPersonaStateChange;
        /// <summary>Fired when a Steam Input controller connects.</summary>
        public static event SteamInputHandleDelegate   OnControllerConnected;
        /// <summary>Fired when a Steam Input controller disconnects.</summary>
        public static event SteamInputHandleDelegate   OnControllerDisconnected;
        /// <summary>Fired when Steam inventory item definitions are updated.</summary>
        public static event Action OnInventoryDefinitionUpdate;
        /// <summary>Fired when a Steam inventory result is ready.</summary>
        public static event SteamInventoryResultReadyDelegate OnInventoryResultReady;
        /// <summary>Fired when a microtransaction authorisation response arrives.</summary>
        public static event SteamMtxTranAuthDelegate OnMicroTxnAuthorisationResponse;
        /// <summary>Fired when the user successfully enters a lobby.</summary>
        public static event SteamLobbyEnterSuccessDelegate OnLobbyEnterSuccess;
        /// <summary>Fired when entering a lobby fails.</summary>
        public static event SteamLobbyEnterFailedDelegate  OnLobbyEnterFailed;
        /// <summary>Fired when lobby or lobby-member data changes.</summary>
        public static event SteamLobbyDataUpdateDelegate   OnLobbyDataUpdate;
        /// <summary>Fired when a raw lobby chat message arrives.</summary>
        public static event SteamLobbyChatMsgDelegate      OnLobbyChatMsg;
        /// <summary>Fired when the user is asked to leave a lobby.</summary>
        public static event SteamLobbyDataDelegate         OnAskedToLeaveLobby;
        /// <summary>Fired when lobby chat membership changes.</summary>
        public static event SteamLobbyChatUpdateDelegate   OnLobbyChatUpdate;
        /// <summary>Fired when a lobby game server is created.</summary>
        public static event SteamLobbyGameServerDelegate   OnLobbyGameServer;
        /// <summary>Fired when the user leaves a lobby.</summary>
        public static event SteamLobbyDataDelegate         OnLobbyLeave;
        /// <summary>Fired when a lobby invitation arrives.</summary>
        public static event SteamLobbyInviteDelegate       OnLobbyInvite;
        /// <summary>Fired when the Steam favourites list changes.</summary>
        public static event SteamFavouritesListChangeDelegate OnFavouritesListChanged;
        /// <summary>Fired when the user requests to join a lobby from the Steam UI.</summary>
        public static event SteamLobbyJoinRequestDelegate  OnLobbyJoinRequested;
        /// <summary>Fired when the Steam game overlay is activated or deactivated.</summary>
        public static event BoolDelegate                   OnGameOverlayActivated;
        /// <summary>Fired when a game server change is requested.</summary>
        public static event SteamGameServerChangeRequestedDelegate OnGameServerChangeRequested;
        /// <summary>Fired when a rich presence join request arrives.</summary>
        public static event SteamRichPresenceJoinRequestedDelegate OnRichPresenceJoinRequested;
        /// <summary>Fired when a party beacon reservation notification arrives.</summary>
        public static event SteamReservationNotificationDelegate   OnReservationNotification;
        /// <summary>Fired when the active beacons list is updated.</summary>
        public static event Action OnActiveBeaconsUpdated;
        /// <summary>Fired when available beacon locations are updated.</summary>
        public static event Action OnAvailableBeaconLocationsUpdated;
        /// <summary>Fired when a Remote Play session connects.</summary>
        public static event SteamRemotePlaySessionIdDelegate OnRemotePlaySessionConnected;
        /// <summary>Fired when a Remote Play session disconnects.</summary>
        public static event SteamRemotePlaySessionIdDelegate OnRemotePlaySessionDisconnected;
        /// <summary>Fired when a local Remote Storage file changes.</summary>
        public static event Action OnRemoteStorageLocalFileChange;
        /// <summary>Fired when a screenshot is ready.</summary>
        public static event SteamScreenshotReadyDelegate OnScreenshotReady;
        /// <summary>Fired when a screenshot is requested.</summary>
        public static event Action OnScreenshotRequested;
        /// <summary>Fired when stats are received for a user.</summary>
        public static event SteamStatsReceivedDelegate OnStatsReceived;
        /// <summary>Fired when a user's stats are unloaded from memory.</summary>
        public static event SteamUserStatsUnloadedDelegate OnStatsUnloaded;
        /// <summary>Fired when a user's achievement is stored.</summary>
        public static event SteamUserAchievementStoredDelegate OnUserAchievementStored;
        /// <summary>Fired when stats are stored to the Steam servers.</summary>
        public static event SteamStatsStoredDelegate OnStatsStored;
        /// <summary>Fired when the app resumes from a suspend state.</summary>
        public static event Action OnAppResumeFromSuspend;
        /// <summary>Fired when the floating keyboard is shown.</summary>
        public static event Action OnKeyboardShown;
        /// <summary>Fired when the floating keyboard is closed.</summary>
        public static event Action OnKeyboardClosed;

        // -----------------------------------------------------------------------
        // Public invoke helpers (called by Foundation API layer; also callable by Toolkit)
        // -----------------------------------------------------------------------

        public static void InvokeOnSteamInitialised()                     => OnSteamInitialised?.Invoke();
        public static void InvokeOnSteamInitialisationError(string msg)   => OnSteamInitialisationError?.Invoke(msg);
        public static void InvokeOnKeyboardShown()                        => OnKeyboardShown?.Invoke();
        public static void InvokeOnGamepadTextInputShown()                => OnGamepadTextInputShown?.Invoke();
        public static void InvokeOnControllerConnected(InputHandle_t h)   => OnControllerConnected?.Invoke(h);
        public static void InvokeOnControllerDisconnected(InputHandle_t h)=> OnControllerDisconnected?.Invoke(h);
        public static void InvokeOnLobbyLeave(LobbyData lobby)            => OnLobbyLeave?.Invoke(lobby);

        // -----------------------------------------------------------------------
        // Callback fields
        // -----------------------------------------------------------------------

        private static Callback<DlcInstalled_t>                      _dlcInstalled;
        private static Callback<NewUrlLaunchParameters_t>            _newUrlLaunchParameters;
        private static Callback<SteamServerConnectFailure_t>         _steamServerConnectFailure;
        private static Callback<SteamServersConnected_t>             _steamServersConnected;
        private static Callback<SteamServersDisconnected_t>          _steamServersDisconnected;
        private static Callback<GamepadTextInputDismissed_t>         _gamepadTextInputDismissed;
        private static Callback<GameConnectedFriendChatMsg_t>        _gameConnectedFriendChatMsg;
        private static Callback<FriendRichPresenceUpdate_t>          _friendRichPresenceUpdate;
        private static Callback<PersonaStateChange_t>                _personaStateChange;
        private static Callback<SteamInventoryDefinitionUpdate_t>    _steamInventoryDefinitionUpdate;
        private static Callback<SteamInventoryResultReady_t>         _steamInventoryResultReady;
        private static Callback<MicroTxnAuthorizationResponse_t>     _microTxnAuthorizationResponse;
        private static Callback<LobbyEnter_t>                        _lobbyEnter;
        private static Callback<LobbyDataUpdate_t>                   _lobbyDataUpdate;
        private static Callback<LobbyChatMsg_t>                      _lobbyChatMsg;
        private static Callback<LobbyChatUpdate_t>                   _lobbyChatUpdate;
        private static Callback<LobbyGameCreated_t>                  _lobbyGameCreated;
        private static Callback<LobbyInvite_t>                       _lobbyInvite;
        private static Callback<FavoritesListChanged_t>              _favouritesListChanged;
        private static Callback<GameLobbyJoinRequested_t>            _gameLobbyJoinRequested;
        private static Callback<GameOverlayActivated_t>              _gameOverlayActivated;
        private static Callback<GameServerChangeRequested_t>         _gameServerChangeRequested;
        private static Callback<GameRichPresenceJoinRequested_t>     _gameRichPresenceJoinRequested;
        private static Callback<ReservationNotificationCallback_t>   _reservationNotificationCallback;
        private static Callback<ActiveBeaconsUpdated_t>              _activeBeaconsUpdated;
        private static Callback<AvailableBeaconLocationsUpdated_t>   _availableBeaconLocationsUpdated;
        private static Callback<SteamRemotePlaySessionConnected_t>   _remotePlaySessionConnected;
        private static Callback<SteamRemotePlaySessionDisconnected_t> _remotePlaySessionDisconnected;
        private static Callback<RemoteStorageLocalFileChange_t>      _remoteStorageLocalFileChange;
        private static Callback<ScreenshotReady_t>                   _screenshotReady;
        private static Callback<ScreenshotRequested_t>               _screenshotRequested;
        private static Callback<UserStatsReceived_t>                 _userStatsReceived;
        private static Callback<UserStatsUnloaded_t>                 _userStatsUnloaded;
        private static Callback<UserStatsStored_t>                   _userStatsStored;
        private static Callback<UserAchievementStored_t>             _userAchievementStored;
        private static Callback<AppResumingFromSuspend_t>            _appResumeFromSuspend;
        private static Callback<FloatingGamepadTextInputDismissed_t> _floatingGamepadTextInputDismissed;

        private static void DisposeCallbacks()
        {
            _dlcInstalled?.Dispose();                       _dlcInstalled = null;
            _newUrlLaunchParameters?.Dispose();             _newUrlLaunchParameters = null;
            _steamServerConnectFailure?.Dispose();          _steamServerConnectFailure = null;
            _steamServersConnected?.Dispose();              _steamServersConnected = null;
            _steamServersDisconnected?.Dispose();           _steamServersDisconnected = null;
            _gamepadTextInputDismissed?.Dispose();          _gamepadTextInputDismissed = null;
            _gameConnectedFriendChatMsg?.Dispose();         _gameConnectedFriendChatMsg = null;
            _friendRichPresenceUpdate?.Dispose();           _friendRichPresenceUpdate = null;
            _personaStateChange?.Dispose();                 _personaStateChange = null;
            _steamInventoryDefinitionUpdate?.Dispose();     _steamInventoryDefinitionUpdate = null;
            _steamInventoryResultReady?.Dispose();          _steamInventoryResultReady = null;
            _microTxnAuthorizationResponse?.Dispose();      _microTxnAuthorizationResponse = null;
            _lobbyEnter?.Dispose();                         _lobbyEnter = null;
            _lobbyDataUpdate?.Dispose();                    _lobbyDataUpdate = null;
            _lobbyChatMsg?.Dispose();                       _lobbyChatMsg = null;
            _lobbyChatUpdate?.Dispose();                    _lobbyChatUpdate = null;
            _lobbyGameCreated?.Dispose();                   _lobbyGameCreated = null;
            _lobbyInvite?.Dispose();                        _lobbyInvite = null;
            _favouritesListChanged?.Dispose();               _favouritesListChanged = null;
            _gameLobbyJoinRequested?.Dispose();             _gameLobbyJoinRequested = null;
            _gameOverlayActivated?.Dispose();               _gameOverlayActivated = null;
            _gameServerChangeRequested?.Dispose();          _gameServerChangeRequested = null;
            _gameRichPresenceJoinRequested?.Dispose();      _gameRichPresenceJoinRequested = null;
            _reservationNotificationCallback?.Dispose();    _reservationNotificationCallback = null;
            _activeBeaconsUpdated?.Dispose();               _activeBeaconsUpdated = null;
            _availableBeaconLocationsUpdated?.Dispose();    _availableBeaconLocationsUpdated = null;
            _remotePlaySessionConnected?.Dispose();         _remotePlaySessionConnected = null;
            _remotePlaySessionDisconnected?.Dispose();      _remotePlaySessionDisconnected = null;
            _remoteStorageLocalFileChange?.Dispose();       _remoteStorageLocalFileChange = null;
            _screenshotReady?.Dispose();                    _screenshotReady = null;
            _screenshotRequested?.Dispose();                _screenshotRequested = null;
            _userStatsReceived?.Dispose();                  _userStatsReceived = null;
            _userStatsUnloaded?.Dispose();                  _userStatsUnloaded = null;
            _userStatsStored?.Dispose();                    _userStatsStored = null;
            _userAchievementStored?.Dispose();              _userAchievementStored = null;
            _appResumeFromSuspend?.Dispose();               _appResumeFromSuspend = null;
            _floatingGamepadTextInputDismissed?.Dispose();  _floatingGamepadTextInputDismissed = null;
        }

        public static void Initialise()
        {
            Debug.Log("Initialising SteamTools Events (Foundation)");

            if (_initialised)
                return;

            _initialised = true;

#if !UNITY_SERVER
            _dlcInstalled                   = Callback<DlcInstalled_t>.Create(p => OnDlcInstalled?.Invoke(p.m_nAppID));
            _newUrlLaunchParameters         = Callback<NewUrlLaunchParameters_t>.Create(_ => OnNewUrlLaunchParameters?.Invoke());
            _steamServerConnectFailure      = Callback<SteamServerConnectFailure_t>.Create(p => OnSteamServerConnectFailure?.Invoke(p.m_eResult, p.m_bStillRetrying));
            _steamServersConnected          = Callback<SteamServersConnected_t>.Create(_ => OnSteamServersConnected?.Invoke());
            _steamServersDisconnected       = Callback<SteamServersDisconnected_t>.Create(p => OnSteamServersDisconnected?.Invoke(p.m_eResult));
            _gamepadTextInputDismissed      = Callback<GamepadTextInputDismissed_t>.Create(p =>
            {
                if (p.m_bSubmitted)
                {
                    SteamUtils.GetEnteredGamepadTextInput(out var text, p.m_unSubmittedText);
                    OnGamepadTextInputDismissed?.Invoke(true, text);
                }
                else
                    OnGamepadTextInputDismissed?.Invoke(false, string.Empty);
            });
            _gameConnectedFriendChatMsg     = Callback<GameConnectedFriendChatMsg_t>.Create(p =>
            {
                SteamFriends.GetFriendMessage(p.m_steamIDUser, p.m_iMessageID, out var text, 8193, out var type);
                OnGameConnectedFriendChatMsg?.Invoke(p.m_steamIDUser, text, type);
            });
            _friendRichPresenceUpdate       = Callback<FriendRichPresenceUpdate_t>.Create(p =>
                OnFriendRichPresenceUpdate?.Invoke(p.m_steamIDFriend, p.m_nAppID));
            _personaStateChange             = Callback<PersonaStateChange_t>.Create(p =>
                OnPersonaStateChange?.Invoke(new CSteamID(p.m_ulSteamID), p.m_nChangeFlags));
            _steamInventoryDefinitionUpdate = Callback<SteamInventoryDefinitionUpdate_t>.Create(_ => OnInventoryDefinitionUpdate?.Invoke());
            _steamInventoryResultReady      = Callback<SteamInventoryResultReady_t>.Create(p => OnInventoryResultReady?.Invoke(p.m_handle));
            _microTxnAuthorizationResponse  = Callback<MicroTxnAuthorizationResponse_t>.Create(p =>
                OnMicroTxnAuthorisationResponse?.Invoke(new AppId_t(p.m_unAppID), p.m_ulOrderID, p.m_bAuthorized == 1));
            _lobbyEnter                     = Callback<LobbyEnter_t>.Create(p =>
            {
                LobbyData lobby = new CSteamID(p.m_ulSteamIDLobby);
                var response    = (EChatRoomEnterResponse)p.m_EChatRoomEnterResponse;
                if (response == EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
                    OnLobbyEnterSuccess?.Invoke(lobby);
                else
                    OnLobbyEnterFailed?.Invoke(lobby, response);
            });
            _lobbyDataUpdate                = Callback<LobbyDataUpdate_t>.Create(p =>
            {
                LobbyData lobby  = new CSteamID(p.m_ulSteamIDLobby);
                UserData  member = new CSteamID(p.m_ulSteamIDMember);
                OnLobbyDataUpdate?.Invoke(lobby, member);
            });
            _lobbyChatMsg                   = Callback<LobbyChatMsg_t>.Create(p =>
            {
                var data        = new byte[4096];
                LobbyData lobby = new CSteamID(p.m_ulSteamIDLobby);
                var ret         = SteamMatchmaking.GetLobbyChatEntry(lobby, (int)p.m_iChatID, out var rawUser, data, data.Length, out var chatEntryType);
                Array.Resize(ref data, ret);
                UserData sender = rawUser;
                OnLobbyChatMsg?.Invoke(lobby, sender, data, chatEntryType);
            });
            _lobbyChatUpdate                = Callback<LobbyChatUpdate_t>.Create(p =>
                OnLobbyChatUpdate?.Invoke((LobbyData)new CSteamID(p.m_ulSteamIDLobby), (UserData)new CSteamID(p.m_ulSteamIDUserChanged), (EChatMemberStateChange)p.m_rgfChatMemberStateChange));
            _lobbyGameCreated               = Callback<LobbyGameCreated_t>.Create(p =>
                OnLobbyGameServer?.Invoke((LobbyData)new CSteamID(p.m_ulSteamIDLobby), new CSteamID(p.m_ulSteamIDGameServer), Utilities.IPUintToString(p.m_unIP), p.m_usPort));
            _lobbyInvite                    = Callback<LobbyInvite_t>.Create(p =>
                OnLobbyInvite?.Invoke((UserData)new CSteamID(p.m_ulSteamIDUser), (LobbyData)new CSteamID(p.m_ulSteamIDLobby), p.m_ulGameID));
            _favouritesListChanged           = Callback<FavoritesListChanged_t>.Create(p =>
                OnFavouritesListChanged?.Invoke(Utilities.IPUintToString(p.m_nIP), Convert.ToUInt16(p.m_nQueryPort), Convert.ToUInt16(p.m_nConnPort), new AppId_t(p.m_nAppID), p.m_nFlags, p.m_bAdd, p.m_unAccountId.m_AccountID));
            _gameLobbyJoinRequested         = Callback<GameLobbyJoinRequested_t>.Create(p =>
                OnLobbyJoinRequested?.Invoke((LobbyData)p.m_steamIDLobby, (UserData)p.m_steamIDFriend));
            _gameOverlayActivated           = Callback<GameOverlayActivated_t>.Create(p =>
                OnGameOverlayActivated?.Invoke(p.m_bActive == 1));
            _gameServerChangeRequested      = Callback<GameServerChangeRequested_t>.Create(p =>
                OnGameServerChangeRequested?.Invoke(p.m_rgchServer, p.m_rgchPassword));
            _gameRichPresenceJoinRequested  = Callback<GameRichPresenceJoinRequested_t>.Create(p =>
                OnRichPresenceJoinRequested?.Invoke(p.m_steamIDFriend, p.m_rgchConnect));
            _reservationNotificationCallback = Callback<ReservationNotificationCallback_t>.Create(p =>
                OnReservationNotification?.Invoke(p.m_steamIDJoiner, p.m_ulBeaconID));
            _activeBeaconsUpdated           = Callback<ActiveBeaconsUpdated_t>.Create(_ => OnActiveBeaconsUpdated?.Invoke());
            _availableBeaconLocationsUpdated = Callback<AvailableBeaconLocationsUpdated_t>.Create(_ => OnAvailableBeaconLocationsUpdated?.Invoke());
            _remotePlaySessionConnected     = Callback<SteamRemotePlaySessionConnected_t>.Create(p => OnRemotePlaySessionConnected?.Invoke(p.m_unSessionID));
            _remotePlaySessionDisconnected  = Callback<SteamRemotePlaySessionDisconnected_t>.Create(p => OnRemotePlaySessionDisconnected?.Invoke(p.m_unSessionID));
            _remoteStorageLocalFileChange   = Callback<RemoteStorageLocalFileChange_t>.Create(_ => OnRemoteStorageLocalFileChange?.Invoke());
            _screenshotReady                = Callback<ScreenshotReady_t>.Create(p => OnScreenshotReady?.Invoke(p.m_hLocal, p.m_eResult));
            _screenshotRequested            = Callback<ScreenshotRequested_t>.Create(_ => OnScreenshotRequested?.Invoke());
            _userStatsReceived              = Callback<UserStatsReceived_t>.Create(p =>
                OnStatsReceived?.Invoke(p.m_nGameID, p.m_eResult, p.m_steamIDUser));
            _userStatsUnloaded              = Callback<UserStatsUnloaded_t>.Create(p =>
                OnStatsUnloaded?.Invoke(p.m_steamIDUser));
            _userStatsStored                = Callback<UserStatsStored_t>.Create(p =>
                OnStatsStored?.Invoke(p.m_nGameID, p.m_eResult));
            _userAchievementStored          = Callback<UserAchievementStored_t>.Create(p =>
                OnUserAchievementStored?.Invoke(new(p)));
            _appResumeFromSuspend           = Callback<AppResumingFromSuspend_t>.Create(_ => OnAppResumeFromSuspend?.Invoke());
            _floatingGamepadTextInputDismissed = Callback<FloatingGamepadTextInputDismissed_t>.Create(_ => OnKeyboardClosed?.Invoke());
#else
            _steamServerConnectFailure  = Callback<SteamServerConnectFailure_t>.CreateGameServer(p => OnSteamServerConnectFailure?.Invoke(p.m_eResult, p.m_bStillRetrying));
            _steamServersConnected      = Callback<SteamServersConnected_t>.CreateGameServer(_ => OnSteamServersConnected?.Invoke());
            _steamServersDisconnected   = Callback<SteamServersDisconnected_t>.CreateGameServer(p => OnSteamServersDisconnected?.Invoke(p.m_eResult));
#endif
        }
    }

    // -----------------------------------------------------------------------
    // Colors
    // -----------------------------------------------------------------------

    /// <summary>
    /// Provides a collection of predefined colour values for use with Steam-related utilities and editors.
    /// </summary>
    public static class Colors
    {
        /// <summary>
        /// A predefined colour value representing the signature blue often associated with Steam branding.
        /// The RGBA value is approximately (0.2, 0.6, 0.93, 1).
        /// </summary>
        public static Color SteamBlue = new(0.2f, 0.60f, 0.93f, 1f);

        /// <summary>
        /// A predefined colour value representing a green shade often associated with Steam branding.
        /// The RGBA value is approximately (0.2, 0.42, 0.2, 1).
        /// </summary>
        public static Color SteamGreen = new(0.2f, 0.42f, 0.2f, 1f);

        /// <summary>
        /// A predefined colour representing a bright green shade with RGBA values (0.4, 0.84, 0.4, 1.0).
        /// </summary>
        public static Color BrightGreen = new(0.4f, 0.84f, 0.4f, 1f);

        /// <summary>
        /// A predefined colour value representing a semi-transparent white with 50% opacity.
        /// The RGBA value is (1, 1, 1, 0.5).
        /// </summary>
        public static Color HalfAlpha = new(1f, 1f, 1f, 0.5f);

        /// <summary>
        /// Represents a colour indicating errors or warnings in the user interface.
        /// RGBA values (1, 0.5, 0.5, 1).
        /// </summary>
        public static Color ErrorRed = new(1, 0.5f, 0.5f, 1);
    }
}
#endif
