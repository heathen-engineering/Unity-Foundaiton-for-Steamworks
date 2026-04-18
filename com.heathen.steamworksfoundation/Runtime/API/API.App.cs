// Copyright 2024 Heathen Engineering Limited
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
//#define UNITY_SERVER
using Steamworks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Exposes a wide range of information and actions for applications and Downloadable Content (DLC).
    /// </summary>
    /// <remarks>
    /// https://partner.steamgames.com/doc/api/ISteamApps
    /// </remarks>
    public static class App
    {
        #region Global
        /// <summary>
        /// Used by Unity Editor to reinitialise the domain
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RunTimeInit()
        {
            _mSteamAPIWarningMessageHook = null;

            Server.Configuration = default;

            if (_callbackWaitThread != null)
            {
                if (_callbackWaitThread.IsBusy)
                {
                    _callbackWaitThread.RunWorkerCompleted -= CallbackWaitThread_RunWorkerCompleted;
                    _callbackWaitThread.ProgressChanged -= CallbackWaitThread_ProgressChanged;

                    _callbackWaitThread.CancelAsync();
                }

                _callbackWaitThread.Dispose();
                _callbackWaitThread = null;
            }

            _suspendCallbacks = false;

            _tickHandlers     = new List<Action>();
            _shutdownHandlers = new List<Action>();
            _unloadHandlers   = new List<Action>();
            InputInitHandler  = null;
        }

        // -----------------------------------------------------------------------
        // Lifecycle hook registration — called by Toolkit subsystems
        // -----------------------------------------------------------------------

        private static List<Action> _tickHandlers     = new();
        private static List<Action> _shutdownHandlers = new();
        private static List<Action> _unloadHandlers   = new();

        /// <summary>
        /// Delegate invoked by <see cref="App.Client.Initialise(AppData,InputActionData[])"/> when input actions are provided.
        /// Toolkit registers its Input.Client.Init logic here; Foundation never references Input directly.
        /// </summary>
        public static Action<InputActionData[]> InputInitHandler;

        /// <summary>Registers a handler to be called each Steam callback tick (after SteamAPI.RunCallbacks).</summary>
        public static void RegisterTickHandler(Action handler)     => _tickHandlers.Add(handler);
        /// <summary>Registers a handler to be called when the application is quitting (before SteamAPI.Shutdown).</summary>
        public static void RegisterShutdownHandler(Action handler) => _shutdownHandlers.Add(handler);
        /// <summary>Registers a handler to be called during Unload to release resources.</summary>
        public static void RegisterUnloadHandler(Action handler)   => _unloadHandlers.Add(handler);

        private static void Application_quitting()
        {
            if (!Initialised)
            {
                return;
            }

            if (_callbackWaitThread != null)
            {
                if (_callbackWaitThread.IsBusy)
                {
                    _callbackWaitThread.RunWorkerCompleted -= CallbackWaitThread_RunWorkerCompleted;
                    _callbackWaitThread.ProgressChanged -= CallbackWaitThread_ProgressChanged;
                    _callbackWaitThread.CancelAsync();
                }

                _callbackWaitThread.Dispose();
                _callbackWaitThread = null;
            }

#if !UNITY_SERVER
            foreach (var h in _shutdownHandlers) h();

            SteamAPI.Shutdown();
#else
            if (Server.Configuration.usingGameServerAuthApi)
                SteamGameServer.SetAdvertiseServerActive(false);

            SteamGameServer.LogOff();

            Steamworks.GameServer.Shutdown();
#endif
            Unload();
        }
        /// <summary>
        /// Unloads the App API from memory releasing images loaded by other API objects
        /// </summary>
        public static void Unload()
        {
            Initialised = false;
            HasInitialisationError = false;
            InitialisationErrorMessage = string.Empty;

            foreach (var h in _unloadHandlers) h();
        }
        /// <summary>
        /// If true, then the system has been initialised
        /// </summary>
        public static bool Initialised { get; private set; }
        /// <summary>
        /// Indicates an error with API initialisation
        /// </summary>
        /// <remarks>
        /// If value is true, then an error occurred during the initialisation of the Steamworks API and normal functionality will not be possible.
        /// </remarks>
        public static bool HasInitialisationError { get; private set; }
        /// <summary>
        /// Initialisation error message if any
        /// </summary>
        /// <remarks>
        /// See <see cref="HasInitialisationError"/> to determine if an error occurred, if so this message will describe possible causes.
        /// </remarks>
        public static string InitialisationErrorMessage { get; private set; }
        /// <summary>
        /// The time in milliseconds between checks of Steam Callbacks
        /// </summary>
        public static int CallbackTickMilliseconds = 1;
        /// <summary>
        /// If set to true, the system will log additional information during execution
        /// </summary>
        public static bool IsDebugging = false;

        private static BackgroundWorker _callbackWaitThread;
        private static bool _suspendCallbacks;

        private static void CallbackWaitThread_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //For later use
        }
        private static void CallbackWaitThread_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
#if !UNITY_SERVER
            SteamAPI.RunCallbacks();
            foreach (var h in _tickHandlers) h();
#else
            Steamworks.GameServer.RunCallbacks();
#endif
        }
        #endregion
        /// <summary>
        /// The AppData indicated by the Steam API. This is the App ID that Valve sees for this app/game.
        /// </summary>
        public static AppData Id => SteamUtils.GetAppID();

        private static SteamAPIWarningMessageHook_t _mSteamAPIWarningMessageHook;
        [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
        private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
        {
            Debug.LogWarning(pchDebugText);
        }

        /// <summary>
        /// Contains App Client API endpoints
        /// </summary>
        public static class Client
        {
            /// <summary>
            /// A Unity Event object to carry <see cref="Steamworks.EResult"/> data for use in the Servers Disconnected event
            /// </summary>
            [Serializable]
            public class UnityEventServersDisconnected : UnityEvent<EResult>
            { }
            /// <summary>
            /// A Unity Even object to carry <see cref="SteamServerConnectFailure"/> data for use in the Servers Connect Failure event
            /// </summary>
            [Serializable]
            public class UnityEventServersConnectFailure : UnityEvent<EResult, bool>
            { }
            /// <summary>
            /// A Unity event triggered when a broadcast upload stops, providing the result of the upload process.
            /// </summary>
            /// <remarks>
            /// This event uses the <see cref="EBroadcastUploadResult"/> enum to relay the outcome of the broadcast upload,
            /// such as success, failure, or specific errors like timeout or bandwidth issues.
            /// </remarks>
            [Serializable]
            public class UnityEventBroadcastUploadStop : UnityEvent<EBroadcastUploadResult>
            { }

            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                _mFileDetailResultT = null;
            }

            /// <summary>
            /// True if the app is connected to the Steam backend, false otherwise.
            /// </summary>
            public static bool LoggedOn => Initialised && SteamUser.BLoggedOn();
            /// <summary>
            /// Start the initialisation process for Steam API given a specific App ID
            /// </summary>
            /// <param name="appId">The ID of the app to request initialisation as</param>
            public static void Initialise(AppData appId) => Initialise(appId, null);

            /// <summary>
            /// Start the initialisation process for Steam API given a specific App ID and artefact settings
            /// </summary>
            /// <param name="appId">The ID of the app to request initialisation as</param>
            /// <param name="actions">A collection of <see cref="InputActionData"/>s to initialise</param>
            public static void Initialise(AppData appId, InputActionData[] actions)
            {
                if (Initialised)
                {
                    HasInitialisationError = true;
                    InitialisationErrorMessage = "Tried to initialize the Steamworks API twice in one session, operation aborted!";
                    SteamTools.Events.InvokeOnSteamInitialisationError(InitialisationErrorMessage);
                    Debug.LogWarning(InitialisationErrorMessage);
                }
                else if (!Packsize.Test())
                {
                    HasInitialisationError = true;
                    InitialisationErrorMessage = "Package size Test returned false, the wrong version of the Steamworks.NET is being run in this platform.";
                    SteamTools.Events.InvokeOnSteamInitialisationError(InitialisationErrorMessage);
                    Debug.LogError(InitialisationErrorMessage);
                }
                else if (!DllCheck.Test())
                {
                    HasInitialisationError = true;
                    InitialisationErrorMessage = "DLL Check Test returned false, one or more of the Steamworks binaries seems to be the wrong version.";
                    SteamTools.Events.InvokeOnSteamInitialisationError(InitialisationErrorMessage);
                    Debug.LogError(InitialisationErrorMessage);
                }
                else
                {
#if !UNITY_SERVER
#if UNITY_EDITOR
                    try
                    {
                        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                        string appIdPath = Path.Combine(projectRoot, "steam_appid.txt");
                        File.WriteAllText(appIdPath, appId.ToString());
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[API.App.Client] Failed to write steam_appid.txt: {e.Message}");
                    }
#endif
                    if (IsDebugging)
                        Debug.Log("Initializing Steam Client API");
                    var result = SteamAPI.InitEx(out var errorMessage);

                    Initialised = result == ESteamAPIInitResult.k_ESteamAPIInitResult_OK;

                    if (!Initialised)
                    { 
                        HasInitialisationError = true;
                        InitialisationErrorMessage = $"Steamworks failed to initialize with result({result}) and message: {errorMessage}";

                        SteamTools.Events.InvokeOnSteamInitialisationError(InitialisationErrorMessage);
                        Debug.LogError(InitialisationErrorMessage);
                    }
                    else
                    {
                        Debug.Log($"Local User: {UserData.Me.Name}:{UserData.Me.Level}");

                        if (_mSteamAPIWarningMessageHook == null)
                        {
                            // Set up our callback to receive warning messages from Steamworks.
                            // You must launch with "-debug_steamapi" in the launch args to receive warnings.
                            _mSteamAPIWarningMessageHook = SteamAPIDebugTextHook;
                            SteamClient.SetWarningMessageHook(_mSteamAPIWarningMessageHook);
                        }

                        if (!SteamUser.BLoggedOn())
                        {
                            Debug.LogWarning(
                                "Steam API was able to initialize however the user does not have an active logon; no real-time services provided by the Steamworks API will be enabled. The Steam client will automatically be trying to recreate the connection as often as possible. When the connection is restored a API.App.Client.EvenServersConnected event will be posted.");
                        }

                        if (IsDebugging)
                        {
                            Debug.Log("Steam API has been initialized with App ID: " + SteamUtils.GetAppID() + ".");
                        }

                        if (appId != SteamUtils.GetAppID())
                        {
#if UNITY_EDITOR
                            Debug.LogWarning(
                                $"The reported application ID of {SteamUtils.GetAppID()} does not match the anticipated ID of {appId}. This is most frequently caused when you edit your AppID but fail to restart Unity, Visual Studio and or any other processes that may have mounted the Steam API under the previous App ID. To correct this please insure your AppID is entered correctly in the SteamSettings object and that you fully restart the Unity Editor, Visual Studio and any other processes that may have connected to them.");
#else
                        Debug.LogError($"The reported AppId is not as expected:\ntAppId Reported = {SteamUtils.GetAppID()}\n\tAppId Expected = {appId}");
                        Application.Quit();
#endif
                        }
                    }
#else
#endif
                    Application.quitting += Application_quitting;
                    Application.focusChanged += Application_focusChanged;

                    if (Initialised)
                    {
                        if (_callbackWaitThread == null)
                        {
                            _callbackWaitThread = new BackgroundWorker();
                            _callbackWaitThread.WorkerSupportsCancellation = true;
                            _callbackWaitThread.WorkerReportsProgress = true;
                            _callbackWaitThread.DoWork += OnCallbackWaitThreadOnDoWork;
                            _callbackWaitThread.RunWorkerCompleted += CallbackWaitThread_RunWorkerCompleted;
                            _callbackWaitThread.ProgressChanged += CallbackWaitThread_ProgressChanged;
                        }

                        _callbackWaitThread.RunWorkerAsync();

                        if (actions is { Length: > 0 })
                            InputInitHandler?.Invoke(actions);

                        SteamTools.Events.InvokeOnSteamInitialised();
                    }
                    else
                    {
                        HasInitialisationError = true;
                        InitialisationErrorMessage = "Steam Initialization failed, check the log for more information.";
                        SteamTools.Events.InvokeOnSteamInitialisationError(InitialisationErrorMessage);
                        Debug.LogError("[Steamworks.NET] Steam Initialization failed, check the log for more information");
                    }
                }
            }

            // ReSharper disable once FunctionNeverReturns
            /// <summary>
            /// This function never returns and is "ended" by the cancellation of the worker
            /// </summary>
            /// <param name="o"></param>
            /// <param name="doWorkEventArgs"></param>
            private static void OnCallbackWaitThreadOnDoWork(object o, DoWorkEventArgs doWorkEventArgs)
            {
                while (true)
                {
                    Thread.Sleep(CallbackTickMilliseconds);

                    if (_suspendCallbacks) continue;

                    _callbackWaitThread.ReportProgress(1);
                }
            }

            private static void Application_focusChanged(bool hasFocus)
            {
                _suspendCallbacks = !hasFocus && !Application.runInBackground;
            }

            private static CallResult<FileDetailsResult_t> _mFileDetailResultT;
            private static Callback<SteamServerConnectFailure_t> _mSteamServerConnectFailureT;
            private static Callback<SteamServersConnected_t> _mSteamServersConnectedT;
            private static Callback<SteamServersDisconnected_t> _mSteamServersDisconnectedT;

            /// <summary>
            /// Checks if the active user is subscribed to the current App ID.
            /// </summary>
            /// <remarks>
            /// NOTE: This will always return true if you're using Steam DRM or calling SteamAPI_RestartAppIfNecessary.
            /// </remarks>
            public static bool IsSubscribed => SteamApps.BIsSubscribed();
            /// <summary>
            /// Checks if the active user is accessing the current appID via a temporary Family Shared licence owned by another user.
            /// </summary>
            public static bool IsSubscribedFromFamilySharing => SteamApps.BIsSubscribedFromFamilySharing();
            /// <summary>
            /// Checks if the user is subscribed to the current App ID through a free weekend.
            /// </summary>
            public static bool IsSubscribedFromFreeWeekend => SteamApps.BIsSubscribedFromFreeWeekend();
            /// <summary>
            /// Checks if the user has a VAC ban on their account
            /// </summary>
            public static bool IsVacBanned => SteamApps.BIsVACBanned();
            /// <summary>
            /// Gets the Steam ID of the original owner of the current app. If it's different from the current user, then it is borrowed.
            /// </summary>
            public static UserData Owner => SteamApps.GetAppOwner();
            /// <summary>
            /// Returns a list of languages supported by the app
            /// </summary>
            public static string[] AvailableLanguages
            {
                get
                {
                    var list = SteamApps.GetAvailableGameLanguages();
                    return list.Split(',');
                }
            }
            /// <summary>
            /// Returns true if a beta branch is being used
            /// </summary>
            public static bool IsBeta => SteamApps.GetCurrentBetaName(out string _, 128);
            /// <summary>
            /// Returns the name of the beta branch being used, if any
            /// </summary>
            public static string CurrentBetaName
            {
                get
                {
                    if (SteamApps.GetCurrentBetaName(out string name, 512))
                        return name;
                    else
                        return string.Empty;
                }
            }
            /// <summary>
            /// Gets the current language that the user has set
            /// </summary>
            public static string CurrentGameLanguage => SteamApps.GetCurrentGameLanguage();
            /// <summary>
            /// Returns the metadata for all available DLC
            /// </summary>
            public static DlcData[] Dlc
            {
                get
                {
                    var count = SteamApps.GetDLCCount();
                    if (count > 0)
                    {
                        var result = new DlcData[count];
                        for (int i = 0; i < count; i++)
                        {
                            if (SteamApps.BGetDLCDataByIndex(i, out AppId_t appid, out bool available, out string name, 512))
                            {
                                result[i] = new DlcData(appid, available, name);
                            }
                            else
                            {
                                Debug.LogWarning("Failed to fetch DLC at index [" + i.ToString() + "]");
                            }
                        }
                        return result;
                    }
                    else
                        return Array.Empty<DlcData>();
                }
            }
            /// <summary>
            /// Checks whether the current App ID is for Cybercafés.
            /// </summary>
            public static bool IsCybercafe => SteamApps.BIsCybercafe();
            /// <summary>
            /// Checks if the licence owned by the user provides low violence depots.
            /// </summary>
            public static bool IsLowViolence => SteamApps.BIsLowViolence();
            /// <summary>
            /// Gets the build id of this app, may change at any time based on backend updates to the game.
            /// </summary>
            public static int BuildId => SteamApps.GetAppBuildId();
            /// <summary>
            /// Gets the installation folder for a specific AppID.
            /// </summary>
            public static string InstallDirectory
            {
                get
                {
                    SteamApps.GetAppInstallDir(SteamUtils.GetAppID(), out string folder, 2048);
                    return folder;
                }
            }
            /// <summary>
            /// Gets the number of DLC pieces for the current app.
            /// </summary>
            public static int DlcCount => SteamApps.GetDLCCount();
            /// <summary>
            /// Gets the command line if the game was launched via Steam URL, e.g. steam://run/&lt;appid&gt;//&lt;command line&gt;/. This method is preferable to launching with a command line via the operating system, which can be a security risk. In order for rich presence joins to go through this and not be placed on the OS command line, you must enable "Use launch command line" from the Installation &gt; General page on your app.
            /// </summary>
            public static string LaunchCommandLine
            {
                get
                {
                    if (
                SteamApps.GetLaunchCommandLine(out string commandline, 512) > 0)
                        return commandline;
                    else
                        return string.Empty;
                }
            }
            /// <summary>
            /// Checks if a specific app is installed.
            /// </summary>
            /// <remarks>
            /// The app may not actually be owned by the current user, they may have it left over from a free weekend, etc.
            /// This only works for base applications, not Downloadable Content(DLC). Use IsDlcInstalled for DLC instead.
            /// </remarks>
            /// <param name="appId">The app to check for</param>
            /// <returns>True if the app is installed</returns>
            public static bool IsAppInstalled(AppData appId) => SteamApps.BIsAppInstalled(appId);
            /// <summary>
            /// Checks if the user owns a specific DLC and if the DLC is installed
            /// </summary>
            /// <param name="appId">The App ID of the DLC to check.</param>
            /// <returns>True if installed</returns>
            public static bool IsDlcInstalled(AppData appId) => SteamApps.BIsDlcInstalled(appId);
            /// <summary>
            /// Gets the download progress for optional DLC.
            /// </summary>
            /// <param name="appId"></param>
            /// <param name="bytesDownloaded"></param>
            /// <param name="bytesTotal"></param>
            /// <returns></returns>
            public static bool GetDlcDownloadProgress(AppData appId, out ulong bytesDownloaded, out ulong bytesTotal) => SteamApps.GetDlcDownloadProgress(appId, out bytesDownloaded, out bytesTotal);
            /// <summary>
            /// Gets the installation directory of the app if any
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static string GetAppInstallDirectory(AppData appId)
            {
                SteamApps.GetAppInstallDir(appId, out string folder, 2048);
                return folder;
            }
            /// <summary>
            /// Returns the collection of installed depots in mount order
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static DepotId_t[] InstalledDepots(AppData appId)
            {
                var results = new DepotId_t[256];
                var count = SteamApps.GetInstalledDepots(appId, results, 256);
                Array.Resize(ref results, (int)count);
                return results;
            }
            /// <summary>
            /// Parameter names starting with the character '@' are reserved for internal use and will always return an empty string. Parameter names starting with an underscore '_' are reserved for steam features -- they can be queried by the game, but it is advised that you not param names beginning with an underscore for your own features.
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public static string QueryLaunchParam(string key) => SteamApps.GetLaunchQueryParam(key);
            /// <summary>
            /// Install an optional DLC
            /// </summary>
            /// <param name="appId"></param>
            public static void InstallDlc(AppData appId) => SteamApps.InstallDLC(appId);
            /// <summary>
            /// Uninstall an optional DLC
            /// </summary>
            /// <param name="appId"></param>
            public static void UninstallDlc(AppData appId) => SteamApps.UninstallDLC(appId);
            /// <summary>
            /// Checks if the active user is subscribed to a specified appId.
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static bool IsSubscribedApp(AppData appId) => SteamApps.BIsSubscribedApp(appId);
            /// <summary>
            /// Checks if the active user is subscribed to a timed trial of the app
            /// </summary>
            /// <param name="secondsAllowed">Total seconds allowed playing</param>
            /// <param name="secondsPlayed">Total seconds that have been played</param>
            /// <returns></returns>
            public static bool IsTimedTrial(out uint secondsAllowed, out uint secondsPlayed) => SteamApps.BIsTimedTrial(out secondsAllowed, out secondsPlayed);
            /// <summary>
            /// Gets the current beta branch name if any
            /// </summary>
            /// <param name="name">outputs the name of the current beta branch, if any</param>
            /// <returns>True if the user is running from a beta branch</returns>
            public static bool GetCurrentBetaName(out string name) => SteamApps.GetCurrentBetaName(out name, 512);
            /// <summary>
            /// Gets the time of purchase of the specified app
            /// </summary>
            /// <param name="appId"></param>
            /// <returns></returns>
            public static DateTime GetEarliestPurchaseTime(AppData appId)
            {
                var secondsSince1970 = SteamApps.GetEarliestPurchaseUnixTime(appId);
                return new DateTime(1970, 1, 1).AddSeconds(secondsSince1970);
            }
            /// <summary>
            /// Asynchronously retrieves metadata details about a specific file in the depot manifest.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="callback"></param>
            public static void GetFileDetails(string name, Action<FileDetailsResult, bool> callback)
            {
                if (callback == null)
                    return;

                _mFileDetailResultT ??= CallResult<FileDetailsResult_t>.Create();

                var handle = SteamApps.GetFileDetails(name);
                _mFileDetailResultT.Set(handle, (r,e) => { callback.Invoke(r, e); });
            }
            /// <summary>
            /// If you detect the game is out-of-date (for example, by having the client detect a version mismatch with a server), you can call use MarkContentCorrupt to force a verify operation, show a message to the user, and then quit.
            /// </summary>
            /// <param name="missingFilesOnly"></param>
            /// <returns></returns>
            public static bool MarkContentCorrupt(bool missingFilesOnly) => SteamApps.MarkContentCorrupt(missingFilesOnly);

        }

        /// <summary>
        /// Contains App Server API endpoints
        /// </summary>
        public static class Server
        {
            /// <summary>
            /// Gets the ID assigned to this server on logon if any
            /// </summary>
            public static CSteamID ID => SteamGameServer.GetSteamID();
            /// <summary>
            /// Is the server logged onto Steam?
            /// </summary>
            // ReSharper disable once UnusedAutoPropertyAccessor.Global
            public static bool LoggedOn { get; private set; }
            /// <summary>
            /// Returns the server's configuration data, see <see cref="SteamGameServerConfiguration"/> for more information.
            /// </summary>
            public static SteamGameServerConfiguration Configuration { get; set; }
            
            private static void OnSteamServersDisconnected(EResult result)
            {
                LoggedOn = false;
                if (IsDebugging)
                    Debug.LogError("Steamworks.GameServer reported connection Closed: " + result.ToString());
            }

            private static void OnSteamServersConnected()
            {
                LoggedOn = true;

                if (IsDebugging)
                    Debug.Log($"Game Server connected to Steamworks successfully!" +
                        $"\n\tMod Directory = {Configuration.gameDirectory}" +
                        $"\n\tApplication ID = {SteamGameServerUtils.GetAppID()}" +
                        $"\n\tServer ID = {SteamGameServer.GetSteamID()}" +
                        $"\n\tServer Name = {Configuration.serverName}" +
                        $"\n\tGame Description = {Configuration.gameDescription}" +
                        $"\n\tMax Player Count = {Configuration.maxPlayerCount}");

                SendUpdatedServerDetailsToSteam();
            }

            private static void OnSteamServerConnectFailure(EResult result, bool retrying)
            {
                LoggedOn = false;
                if (IsDebugging)
                    Debug.LogError("Steamworks.GameServer.LogOn reported connection Failure: " + result.ToString());
            }
            /// <summary>
            /// Initialise the Steam Game Server API with the provided configuration settings
            /// </summary>
            /// <param name="appId">The App to initialize as</param>
            /// <param name="serverConfiguration">The configuration settings to apply</param>
            public static void Initialise(AppData appId, SteamGameServerConfiguration serverConfiguration)
            {
                if (Initialised)
                {
                    HasInitialisationError = true;
                    InitialisationErrorMessage = "Tried to initialize the Steamworks API twice in one session, operation aborted!";
                    SteamTools.Events.InvokeOnSteamInitialisationError(InitialisationErrorMessage);
                    Debug.LogWarning(InitialisationErrorMessage);
                }
                else if (!Packsize.Test())
                {
                    HasInitialisationError = true;
                    InitialisationErrorMessage = "Package size Test returned false, the wrong version of the Steamworks.NET is being run in this platform.";
                    SteamTools.Events.InvokeOnSteamInitialisationError(InitialisationErrorMessage);
                    Debug.LogError(InitialisationErrorMessage);
                }
                else if (!DllCheck.Test())
                {
                    HasInitialisationError = true;
                    InitialisationErrorMessage = "DLL Check Test returned false, one or more of the Steamworks binaries seems to be the wrong version.";
                    SteamTools.Events.InvokeOnSteamInitialisationError(InitialisationErrorMessage);
                    Debug.LogError(InitialisationErrorMessage);
                }
                else
                {

                    Configuration = serverConfiguration;

                    if (IsDebugging)
                        Debug.Log("Registering Steam Game Server callbacks.");

                    RegisterCallbacks();

                    EServerMode eMode = EServerMode.eServerModeNoAuthentication;

                    if (serverConfiguration.usingGameServerAuthApi)
                        eMode = EServerMode.eServerModeAuthenticationAndSecure;

#if UNITY_EDITOR
                    try
                    {
                        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                        string appIdPath = Path.Combine(projectRoot, "steam_appid.txt");
                        File.WriteAllText(appIdPath, appId.ToString());
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[API.App.Server] Failed to write steam_appid.txt: {e.Message}");
                    }
#endif

                    if (IsDebugging)
                        Debug.Log("Initializing Steam Game Server API: (" + serverConfiguration.ip + ", " + serverConfiguration.gamePort.ToString() + ", " + serverConfiguration.queryPort.ToString() + ", " + eMode.ToString() + ", " + serverConfiguration.serverVersion + ")");

                    Initialised = GameServer.Init(serverConfiguration.ip, serverConfiguration.gamePort, serverConfiguration.queryPort, eMode, serverConfiguration.serverVersion);

                    if (!Initialised)
                    {
                        HasInitialisationError = true;
                        InitialisationErrorMessage = "Steam API failed to initialize!\nOne of the following issues must be true:\n"
                                + "- The Steam couldn't determine the App ID of the game. If you're running your server from the executable or debugger directly then you must have a steam_appid.txt in your server directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the steam_appid.txt file.\n"
                                + "- The Game port and or Query port could not be bound.\n"
                                + "- The App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.";

                        SteamTools.Events.InvokeOnSteamInitialisationError(InitialisationErrorMessage);
                        Debug.LogError(InitialisationErrorMessage);
                    }
                    else
                    {
                        if (IsDebugging)
                        {
                            if (Configuration.DebugValidate())
                                Debug.Log($"Applying Steam Game Server Settings:" +
                                    $"\n\tSetModDir: {serverConfiguration.gameDirectory}" +
                                    $"\n\tSetProduct: {appId}" +
                                    $"\n\tSetGameDescription: {serverConfiguration.gameDescription}" +
                                    $"\n\tSetMaxPlayerCount: {serverConfiguration.maxPlayerCount}" +
                                    $"\n\tSetPasswordProtected: {serverConfiguration.isPasswordProtected}" +
                                    $"\n\tSetServerName: {serverConfiguration.serverName}" +
                                    $"\n\tSetBotPlayerCount: {serverConfiguration.botPlayerCount}" +
                                    $"\n\tSetMapName: {serverConfiguration.mapName}" +
                                    $"\n\tSetDedicatedServer: {serverConfiguration.isDedicated}");
                        }

                        SteamGameServer.SetModDir(serverConfiguration.gameDirectory);
                        SteamGameServer.SetProduct(appId.ToString());
                        SteamGameServer.SetGameDescription(serverConfiguration.gameDescription);
                        SteamGameServer.SetMaxPlayerCount(serverConfiguration.maxPlayerCount);
                        SteamGameServer.SetPasswordProtected(serverConfiguration.isPasswordProtected);
                        SteamGameServer.SetServerName(serverConfiguration.serverName);
                        SteamGameServer.SetBotPlayerCount(serverConfiguration.botPlayerCount);
                        SteamGameServer.SetMapName(serverConfiguration.mapName);
                        SteamGameServer.SetDedicatedServer(serverConfiguration.isDedicated);

                        if (serverConfiguration.supportSpectators)
                        {
                            if (IsDebugging)
                                Debug.Log("Spectator enabled:\n\tName = " + serverConfiguration.spectatorServerName + "\n\tSpectator Port = " + serverConfiguration.spectatorPort.ToString());

                            SteamGameServer.SetSpectatorPort(serverConfiguration.spectatorPort);
                            SteamGameServer.SetSpectatorServerName(serverConfiguration.spectatorServerName);
                        }
                        else if (IsDebugging)
                            Debug.Log("Spectator Set Up Skipped");

                        if (IsDebugging)
                        {
                            Debug.Log("Steam API has been initialized with App ID: " + SteamGameServerUtils.GetAppID());
                        }

                        if (appId != SteamGameServerUtils.GetAppID())
                        {
#if UNITY_EDITOR
                            Debug.LogWarning($"The reported application ID of {SteamGameServerUtils.GetAppID()} does not match the anticipated ID of {appId}. This is most frequently caused when you edit your AppID bu fail to restart Unity, Visual Studio and or any other processes that may have mounted the Steam API under the previous App ID. To correct this please insure your AppID is entered correctly in the SteamSettings object and that you fully restart the Unity Editor, Visual Studio and any other processes that may have connected to them.");
#else
                            Debug.LogError($"The reported AppId is not as expected:\nAppId Reported = {SteamGameServerUtils.GetAppID()}, AppId Expected = {appId}");
#endif
                        }
                    }

                    Application.quitting += Application_quitting;

                    if (Initialised)
                    {
                        if (_callbackWaitThread == null)
                        {
                            _callbackWaitThread = new BackgroundWorker();
                            _callbackWaitThread.WorkerSupportsCancellation = true;
                            _callbackWaitThread.WorkerReportsProgress = true;
                            _callbackWaitThread.DoWork += OnCallbackWaitThreadOnDoWork;
                            _callbackWaitThread.RunWorkerCompleted += CallbackWaitThread_RunWorkerCompleted;
                            _callbackWaitThread.ProgressChanged += CallbackWaitThread_ProgressChanged;
                        }

                        _callbackWaitThread.RunWorkerAsync();

                        SteamTools.Events.InvokeOnSteamInitialised();

                        if (Configuration.autoLogon)
                            LogOn();
                    }
                    else
                    {
                        HasInitialisationError = true;
                        InitialisationErrorMessage = "Steam Initialization failed, check the log for more information.";
                        SteamTools.Events.InvokeOnSteamInitialisationError(InitialisationErrorMessage);
                        Debug.LogError("[Steamworks.NET] Steam Initialization failed, check the log for more information");
                    }
                }
            }

            private static void OnCallbackWaitThreadOnDoWork(object o, DoWorkEventArgs doWorkEventArgs)
            {
                while (true)
                {
                    Thread.Sleep(CallbackTickMilliseconds);
                    _callbackWaitThread.ReportProgress(1);
                }
                // ReSharper disable once FunctionNeverReturns
            }

            /// <summary>
            /// Log the server on to the Steam Game Server API end points
            /// </summary>
            public static void LogOn()
            {
                if (Configuration.anonymousServerLogin)
                {
                    if (IsDebugging)
                        Debug.Log("Logging on with Anonymous");

                    SteamGameServer.LogOnAnonymous();
                }
                else
                {
                    if (IsDebugging)
                        Debug.Log("Logging on with token");

                    SteamGameServer.LogOn(Configuration.gameServerToken);
                }

                // We want to actively update the master server with our presence so players can
                // find us via the steam matchmaking/server browser interfaces
                if (Configuration.usingGameServerAuthApi || Configuration.enableHeartbeats)
                {
                    if (IsDebugging)
                        Debug.Log("Enabling server heartbeat.");

                    SteamGameServer.SetAdvertiseServerActive(true);
                }

                Debug.Log("Steamworks Game Server Started.\nWaiting for connection result from Steamworks");
            }
            /// <summary>
            /// Update server details on the Steam Game Server API
            /// </summary>
            public static void SendUpdatedServerDetailsToSteam()
            {
                if (Configuration.rulePairs != null && Configuration.rulePairs.Length > 0)
                {
                    var pairString = "Set the following rules:\n";

                    foreach (var pair in Configuration.rulePairs)
                    {
                        SteamGameServer.SetKeyValue(pair.key, pair.value);
                        pairString += "\n\t[" + pair.key + "] = [" + pair.value + "]";
                    }

                    if (IsDebugging)
                        Debug.Log(pairString);
                }
            }
            /// <summary>
            /// Configures Steam Game Server API callbacks
            /// </summary>
            public static void RegisterCallbacks()
            {
                SteamTools.Events.OnSteamServersConnected += OnSteamServersConnected;
                SteamTools.Events.OnSteamServersDisconnected += OnSteamServersDisconnected;
                SteamTools.Events.OnSteamServerConnectFailure += OnSteamServerConnectFailure;
            }
        }
    }
}
#endif
