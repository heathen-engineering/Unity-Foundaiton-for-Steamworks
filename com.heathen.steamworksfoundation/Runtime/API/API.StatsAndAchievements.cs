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
using Steamworks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Provides access to functionality related to player stats and achievements,
    /// including retrieving, updating, and clearing achievement data, as well as handling related events.
    /// </summary>
    public static class StatsAndAchievements
    {
        /// <summary>
        /// Provides access to client-related functionality for managing stats and achievements.
        /// This includes handling events such as user stats received, user stats unloaded,
        /// user stats stored, and achievement updates. The class also provides methods to
        /// manage achievements, retrieve their attributes, access global statistics, and handle
        /// various details about player achievements and stats.
        /// </summary>
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                _pendingLinks = new();

                if(_loadedImages != null)
                {
                    foreach(var kvp in _loadedImages)
                    {
                        var target = kvp.Value;
                        Object.Destroy(target);
                    }
                }

                _loadedImages = new();
                _onUserStatsReceived = new();
                _onUserStatsUnloaded = new();
                _onUserStatsStored = new();
                _onUserAchievementStored = new();
                _onAchievementStatusChanged = new();

                _userStatsReceivedT = null;
                _userStatsUnloadedT = null;
                _userStatsStoredT = null;
                _userAchievementStoredT = null;
                _userAchievementIconFetchedT = null;
                _numberOfCurrentPlayersT = null;
                _globalAchievementPercentagesReadyT = null;
                _globalStatsReceivedT = null;
                _userStatsReceivedT2 = null;
            }

            private class ImageRequestCallbackLink
            {
                public bool IsAchievement;
                public string APIName;
                public Action<Texture2D> Callback;
            }

            /// <summary>
            /// Invoked when the current user's stats have been received from the server.
            /// </summary>
            public static UnityEvent<UserStatsReceived> OnUserStatsReceived
            {
                get
                {
                    _userStatsReceivedT ??= Callback<UserStatsReceived_t>.Create(r => _onUserStatsReceived?.Invoke(r));
                    return _onUserStatsReceived;
                }
            }

            /// <summary>
            /// Invoked when another user's stats have been unloaded from the cache.
            /// </summary>
            public static UnityEvent<UserStatsUnloaded_t> OnUserStatsUnloaded
            {
                get
                {
                    _userStatsUnloadedT ??= Callback<UserStatsUnloaded_t>.Create(r => _onUserStatsUnloaded?.Invoke(r));
                    return _onUserStatsUnloaded;
                }
            }

            /// <summary>
            /// Invoked when the current user's stats have been stored to the server.
            /// </summary>
            public static UnityEvent<UserStatsStored_t> OnUserStatsStored
            {
                get
                {
                    _userStatsStoredT ??= Callback<UserStatsStored_t>.Create(r => _onUserStatsStored?.Invoke(r));
                    return _onUserStatsStored;
                }
            }

            /// <summary>
            /// Invoked when an achievement has been stored for the current user.
            /// </summary>
            public static UnityEvent<UserAchievementStored_t> OnUserAchievementStored
            {
                get
                {
                    _userAchievementStoredT ??= Callback<UserAchievementStored_t>.Create(r => _onUserAchievementStored?.Invoke(r));
                    return _onUserAchievementStored;
                }
            }

            /// <summary>
            /// Triggered when the status of an achievement changes.
            /// This event provides the achievement API name and its current status (unlocked or locked).
            /// </summary>
            public static UnityEvent<string, bool> OnAchievementStatusChanged => _onAchievementStatusChanged;

            private static List<ImageRequestCallbackLink> _pendingLinks = new();
            private static Dictionary<int, Texture2D> _loadedImages = new();
            private static UnityEvent<UserStatsReceived> _onUserStatsReceived = new();
            private static UnityEvent<UserStatsUnloaded_t> _onUserStatsUnloaded = new();
            private static UnityEvent<UserStatsStored_t> _onUserStatsStored = new();
            private static UnityEvent<UserAchievementStored_t> _onUserAchievementStored = new();
            private static UnityEvent<string, bool> _onAchievementStatusChanged = new();

            private static Callback<UserStatsReceived_t> _userStatsReceivedT;
            private static Callback<UserStatsUnloaded_t> _userStatsUnloadedT;
            private static Callback<UserStatsStored_t> _userStatsStoredT;
            private static Callback<UserAchievementStored_t> _userAchievementStoredT;
            private static Callback<UserAchievementIconFetched_t> _userAchievementIconFetchedT;

            private static CallResult<NumberOfCurrentPlayers_t> _numberOfCurrentPlayersT;
            private static CallResult<GlobalAchievementPercentagesReady_t> _globalAchievementPercentagesReadyT;
            private static CallResult<GlobalStatsReceived_t> _globalStatsReceivedT;
            private static CallResult<UserStatsReceived_t> _userStatsReceivedT2;

            /// <summary>
            /// Resets the completion status of the specified achievement.
            /// </summary>
            /// <param name="achievementApiName">The API name of the achievement to reset.</param>
            /// <returns>True if the achievement was successfully reset; otherwise, false.</returns>
            public static bool ClearAchievement(string achievementApiName)
            {
                var value = SteamUserStats.ClearAchievement(achievementApiName);

                if (value)
                {
                    OnAchievementStatusChanged?.Invoke(achievementApiName,
                        GetAchievement(achievementApiName, out var status) && status);
                }

                return value;
            }
            /// <summary>
            /// Gets the unlocked status of the Achievement.
            /// </summary>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="achieved">Returns the unlocked status of the achievement.</param>
            /// <returns></returns>
            public static bool GetAchievement(string achievementApiName, out bool achieved) => SteamUserStats.GetAchievement(achievementApiName, out achieved);
            /// <summary>
            /// Returns the percentage of users who have unlocked the specified achievement.
            /// </summary>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="achieved">Returns whether the current user has unlocked the achievement.</param>
            /// <param name="unlockTime">Returns the time that the achievement was unlocked;</param>
            /// <returns></returns>
            public static bool GetAchievement(string achievementApiName, out bool achieved, out DateTime unlockTime)
            {
                var result = SteamUserStats.GetAchievementAndUnlockTime(achievementApiName, out achieved, out uint epoch);
                unlockTime = new DateTime(1970, 1, 1).AddSeconds(epoch);
                return result;
            }
            /// <summary>
            /// Gets the unlocked status of the Achievement.
            /// </summary>
            /// <param name="userId">The Steam ID of the user to get the achievement for.</param>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="achieved">Returns the unlocked status of the achievement.</param>
            /// <returns></returns>
            public static bool GetAchievement(UserData userId, string achievementApiName, out bool achieved) => SteamUserStats.GetUserAchievement(userId, achievementApiName, out achieved);
            /// <summary>
            /// Gets the achievement status, and the time it was unlocked if unlocked.
            /// </summary>
            /// <param name="userId"></param>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="achieved">Returns the unlocked status of the achievement.</param>
            /// <param name="unlockTime">Returns the time that the achievement was unlocked.</param>
            /// <returns></returns>
            public static bool GetAchievement(UserData userId, string achievementApiName, out bool achieved, out DateTime unlockTime)
            {
                var result = SteamUserStats.GetUserAchievementAndUnlockTime(userId, achievementApiName, out achieved, out uint epoch);
                unlockTime = new DateTime(1970, 1, 1).AddSeconds(epoch);
                return result;
            }
            /// <summary>
            /// Returns the percentage of users who have unlocked the specified achievement.
            /// </summary>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="percent">Variable to return the percentage of people that have unlocked this achievement from 0 to 100.</param>
            /// <returns></returns>
            public static bool GetAchievementAchievedPercent(string achievementApiName, out float percent) => SteamUserStats.GetAchievementAchievedPercent(achievementApiName, out percent);
            /// <summary>
            /// Get general attributes for an achievement. Currently, provides: Name, Description, and Hidden status.
            /// </summary>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="key">The 'key' to get a value for.</param>
            /// <returns></returns>
            public static string GetAchievementDisplayAttribute(string achievementApiName, string key) => SteamUserStats.GetAchievementDisplayAttribute(achievementApiName, key);
            /// <summary>
            /// Get general attributes for an achievement. Currently, provides: Name, Description, and Hidden status.
            /// </summary>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="attribute">The 'attribute' to get a value for.</param>
            /// <returns></returns>
            public static string GetAchievementDisplayAttribute(string achievementApiName, AchievementAttributes attribute) => SteamUserStats.GetAchievementDisplayAttribute(achievementApiName, attribute.ToString());
            /// <summary>
            /// Gets the icon for an achievement.
            /// </summary>
            /// <param name="achievementApiName"></param>
            /// <param name="callback"></param>
            /// <returns>True if request was successfully sent to Steam, if False this indicates that either the API name is not valid or that User Stats have not been requested, please check <see cref="UserStatsLoaded"/> before working with achievements or stats.</returns>
            public static bool GetAchievementIcon(string achievementApiName, Action<Texture2D> callback)
            {
                //Valve seems to sometimes not register the icon fetched callback unless a read has been done on the achievement ... so do a read then request
                if (!SteamUserStats.GetAchievement(achievementApiName, out _))
                {
                    return false;
                }

                if (callback == null)
                {
                    Debug.LogWarning(nameof(GetAchievementIcon) + ": Callback cannot be null.");
                    return false;
                }

                _userAchievementIconFetchedT ??= Callback<UserAchievementIconFetched_t>.Create(HandleIconImageLoaded);

                var handle = SteamUserStats.GetAchievementIcon(achievementApiName);
                if (handle > 0)
                {
                    if (_loadedImages.TryGetValue(handle, out var image))
                        callback.Invoke(image);
                    else
                    {
                        if (LoadImage(handle))
                            callback.Invoke(_loadedImages[handle]);
                        else
                        {
                            Debug.LogWarning(nameof(GetAchievementIcon) + ": Failed to load the requested icon");
                            callback.Invoke(null);
                        }
                    }
                }
                else
                {
                    _pendingLinks.Add(new ImageRequestCallbackLink
                    {
                        IsAchievement = true,
                        APIName = achievementApiName,
                        Callback = callback
                    });
                }

                return true;
            }
            /// <summary>
            /// Gets the 'API name' for an achievement index between 0 and GetNumAchievements.
            /// </summary>
            /// <param name="index"></param>
            /// <returns></returns>
            public static string GetAchievementName(uint index) => SteamUserStats.GetAchievementName(index);
            /// <summary>
            /// Gets a list of all achievement names registered to the app
            /// </summary>
            /// <returns></returns>
            public static string[] GetAchievementNames()
            {
                var count = SteamUserStats.GetNumAchievements();
                var results = new string[count];
                for (int i = 0; i < count; i++)
                {
                    results[i] = SteamUserStats.GetAchievementName((uint)i);
                }
                return results;
            }
            
            /// <summary>
            /// Gets the lifetime totals for an aggregated stat.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns></returns>
            public static bool GetGlobalStat(string statApiName, out long data) => SteamUserStats.GetGlobalStat(statApiName, out data);
            /// <summary>
            /// Gets the lifetime totals for an aggregated stat.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns></returns>
            public static bool GetGlobalStat(string statApiName, out double data) => SteamUserStats.GetGlobalStat(statApiName, out data);
            
            public static void GetMostAchievedAchievements(Action<EResult, (AchievementData achievement, float percentage)[], bool> callback)
            {
                RequestGlobalAchievementPercentages((result, error) =>
                {
                    if (!error && result.m_eResult == EResult.k_EResultOK)
                    {
                        var index = GetMostAchievedAchievementInfo(out string achievementApiName, out float percent, out _);
                        if (index > -1)
                        {
                            List<(AchievementData achievement, float percentage)> results = new List<(AchievementData achievement, float percentage)>();

                            while (index != -1)
                            {
                                results.Add((achievementApiName, percent));

                                index = GetNextMostAchievedAchievementInfo(index, out achievementApiName, out percent, out _);
                            }

                            callback?.Invoke(result.m_eResult, results.ToArray(), false);
                        }
                        else
                            callback?.Invoke(result.m_eResult, null, false);
                    }
                    else
                        callback?.Invoke(result.m_eResult, null, error);
                });
            }
            /// <summary>
            /// Gets the info on the most achieved achievement for the game.
            /// </summary>
            /// <param name="achievementApiName"></param>
            /// <param name="percent"></param>
            /// <param name="achieved"></param>
            /// <returns>Returns -1 if RequestGlobalAchievementPercentages has not been called or if there are no global achievement percentages for this app ID. If the call is successful, it returns an iterator which should be used with GetNextMostAchievedAchievementInfo.</returns>
            public static int GetMostAchievedAchievementInfo(out string achievementApiName, out float percent, out bool achieved) => SteamUserStats.GetMostAchievedAchievementInfo(out achievementApiName, 8193, out percent, out achieved);
            /// <summary>
            /// Gets the info on the next most achieved achievement for the game.
            /// </summary>
            /// <remarks>
            /// You must have called RequestGlobalAchievementPercentages, and it needs to return successfully via its callback before calling this.
            /// </remarks>
            /// <param name="previousIndex">Iterator returned from the previous call to this function or from GetMostAchievedAchievementInfo</param>
            /// <param name="achievementApiName">String buffer to return the 'API Name' of the achievement into.</param>
            /// <param name="percent">Variable to return the percentage of people that have unlocked this achievement from 0 to 100.</param>
            /// <param name="achieved">Variable to return whether the current user has unlocked this achievement.</param>
            /// <returns></returns>
            public static int GetNextMostAchievedAchievementInfo(int previousIndex, out string achievementApiName, out float percent, out bool achieved) => SteamUserStats.GetNextMostAchievedAchievementInfo(previousIndex, out achievementApiName, 8193, out percent, out achieved);
            /// <summary>
            /// Get the number of achievements defined in the App Admin panel of the Steamworks website.
            /// </summary>
            /// <returns></returns>
            public static uint GetNumAchievements() => SteamUserStats.GetNumAchievements();
            /// <summary>
            /// Asynchronously retrieves the total number of players currently playing the current game. Both online and in offline mode.
            /// </summary>
            /// <param name="callback"></param>
            public static void GetNumberOfCurrentPlayers(Action<NumberOfCurrentPlayers_t, bool> callback)
            {
                if (callback == null)
                    return;

                _numberOfCurrentPlayersT ??= CallResult<NumberOfCurrentPlayers_t>.Create();

                var handle = SteamUserStats.GetNumberOfCurrentPlayers();
                _numberOfCurrentPlayersT.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Gets the current value of a stat for the current user.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns></returns>
            public static bool GetStat(string statApiName, out int data) => SteamUserStats.GetStat(statApiName, out data);
            /// <summary>
            /// Gets the current value of a stat for the current user.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns></returns>
            public static bool GetStat(string statApiName, out float data) => SteamUserStats.GetStat(statApiName, out data);
            /// <summary>
            /// Gets the current value of a stat for the specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user to get the stat for.</param>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns></returns>
            public static bool GetStat(UserData userId, string statApiName, out int data) => SteamUserStats.GetUserStat(userId, statApiName, out data);
            /// <summary>
            /// Gets the current value of a stat for the specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user to get the stat for.</param>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The variable to return the stat value into.</param>
            /// <returns></returns>
            public static bool GetStat(UserData userId, string statApiName, out float data) => SteamUserStats.GetUserStat(userId, statApiName, out data);
            /// <summary>
            /// Shows the user a pop-up notification with the current progress of an achievement.
            /// </summary>
            /// <remarks>
            /// Calling this function will NOT set the progress or unlock the achievement, the game must do that manually by calling SetStat!
            /// </remarks>
            /// <param name="achievementApiName">The 'API Name' of the achievement.</param>
            /// <param name="progress">The current progress.</param>
            /// <param name="maxProgress">The progress required to unlock the achievement.</param>
            /// <returns></returns>
            public static bool IndicateAchievementProgress(string achievementApiName, uint progress, uint maxProgress) => SteamUserStats.IndicateAchievementProgress(achievementApiName, progress, maxProgress);
            /// <summary>
            /// Asynchronously fetch the data for the percentage of players who have received each achievement for the current game globally.
            /// </summary>
            /// <remarks>
            /// You must have called RequestCurrentStats, and it needs to return successfully via its callback before calling this!
            /// </remarks>
            public static void RequestGlobalAchievementPercentages(Action<GlobalAchievementPercentagesReady_t, bool> callback)
            {
                if (callback == null)
                    return;

                _globalAchievementPercentagesReadyT ??= CallResult<GlobalAchievementPercentagesReady_t>.Create();

                var handle = SteamUserStats.RequestGlobalAchievementPercentages();
                _globalAchievementPercentagesReadyT.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Asynchronously fetches global stats data, which is available for stats marked as "aggregated" in the App Admin panel of the Steamworks website.
            /// </summary>
            /// <param name="historyDays">How many days of day-by-day history to retrieve in addition to the overall totals. The limit is 60.</param>
            /// <param name="callback"></param>
            public static void RequestGlobalStats(int historyDays, Action<GlobalStatsReceived_t, bool> callback)
            {
                if (callback == null)
                    return;

                _globalStatsReceivedT ??= CallResult<GlobalStatsReceived_t>.Create();

                var handle = SteamUserStats.RequestGlobalStats(historyDays);
                _globalStatsReceivedT.Set(handle, callback.Invoke);
            }
            /// <summary>
            /// Asynchronously downloads stats and achievements for the specified user from the server.
            /// </summary>
            /// <remarks>
            /// To keep from using too much memory, the least recently used cache (LRU) is maintained and the other user's stats will occasionally be unloaded. When this happens, a UserStatsUnloaded_t callback is sent. After receiving this callback, the user's stats will be unavailable until this function is called again.
            /// </remarks>
            /// <param name="userId"></param>
            /// <param name="callback"></param>
            public static void RequestUserStats(UserData userId, Action<UserStatsReceived, bool> callback)
            {
                if (callback == null)
                    return;

                _userStatsReceivedT2 ??= CallResult<UserStatsReceived_t>.Create();

                var handle = SteamUserStats.RequestUserStats(userId);
                _userStatsReceivedT2.Set(handle, (r,e) => callback.Invoke(r, e));
            }
            /// <summary>
            /// Resets the current users stats and, optionally, achievements.
            /// </summary>
            /// <remarks>
            /// This automatically calls StoreStats to persist the changes to the server. This should typically only be used for testing purposes during development. Ensure that you sync up your stats with the new default values provided by Steam after calling this by calling RequestCurrentStats.
            /// </remarks>
            /// <param name="achievementsToo"></param>
            /// <returns></returns>
            public static bool ResetAllStats(bool achievementsToo)
            {
                var value = SteamUserStats.ResetAllStats(achievementsToo);
                if (value)
                {
                    foreach (var achievementApiName in GetAchievementNames())
                    {
                        OnAchievementStatusChanged?.Invoke(achievementApiName,
                            GetAchievement(achievementApiName, out bool status) && status);
                    }
                }

                return value;
            }
            /// <summary>
            /// Unlocks an achievement.
            /// </summary>
            /// <param name="achievementApiName"></param>
            /// <returns></returns>
            public static bool SetAchievement(string achievementApiName)
            {
                var value = SteamUserStats.SetAchievement(achievementApiName);

                if (value)
                {
                    OnAchievementStatusChanged?.Invoke(achievementApiName,
                        GetAchievement(achievementApiName, out var status) && status);
                }

                return value;
            }
            /// <summary>
            /// Sets / updates the value of a given stat for the current user.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The new value of the stat. This must be an absolute value, it will not increment or decrement for you.</param>
            /// <returns></returns>
            public static bool SetStat(string statApiName, int data) => SteamUserStats.SetStat(statApiName, data);
            /// <summary>
            /// Sets / updates the value of a given stat for the current user.
            /// </summary>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than k_cchStatNameMax.</param>
            /// <param name="data">The new value of the stat. This must be an absolute value, it will not increment or decrement for you.</param>
            /// <returns></returns>
            public static bool SetStat(string statApiName, float data) => SteamUserStats.SetStat(statApiName, data);

            /// <summary>
            /// Sends updated stats and achievement data to the Steam servers for permanent storage.
            /// </summary>
            /// <remarks>
            /// This method ensures that changes to stats and achievements are sent to the server.
            /// It should be called sparingly, as frequent calls may cause rate-limiting.
            /// Recommended usage scenarios include major state transitions, such as the end of a session or significant gameplay events.
            /// Additionally, it is necessary to invoke this method shortly after calling <c>SetAchievement</c> to display the achievement notification dialogue.
            /// </remarks>
            /// <returns>True if the data was successfully stored on the server; otherwise, false.</returns>
            public static bool StoreStats() => SteamUserStats.StoreStats();

            /// <summary>
            /// Updates an AVGRATE stat with the specified values.
            /// </summary>
            /// <param name="statApiName">The API name of the stat. Must not exceed the maximum allowable length.</param>
            /// <param name="countThisSession">The accumulated value to add since the last update.</param>
            /// <param name="sessionLength">The elapsed time in seconds since the last update.</param>
            /// <returns>True if the stat was successfully updated; otherwise, false.</returns>
            public static bool UpdateAvgRateStat(string statApiName, float countThisSession, double sessionLength) =>
                SteamUserStats.UpdateAvgRateStat(statApiName, countThisSession, sessionLength);

            private static void HandleIconImageLoaded(UserAchievementIconFetched_t param)
            {
                if (LoadImage(param.m_nIconHandle))
                {
                    var target = _loadedImages[param.m_nIconHandle];
                    var apiName = param.m_rgchAchievementName;
                    foreach (var link in _pendingLinks)
                    {
                        if (link.IsAchievement && link.APIName == apiName)
                            link.Callback?.Invoke(target);
                    }

                    _pendingLinks.RemoveAll(p => p.IsAchievement && p.APIName == apiName);
                }
                else
                {
                    var apiName = param.m_rgchAchievementName;
                    foreach (var link in _pendingLinks)
                    {
                        if (link.IsAchievement && link.APIName == apiName)
                            link.Callback?.Invoke(null);
                    }

                    _pendingLinks.RemoveAll(p => p.IsAchievement && p.APIName == apiName);
                }
            }
            private static bool LoadImage(int imageHandle)
            {
                if (SteamUtils.GetImageSize(imageHandle, out uint width, out uint height))
                {
                    Texture2D pointer = null;

                    if (_loadedImages.TryGetValue(imageHandle, out var image))
                        pointer = image;

                    if (pointer == null)
                    {
                        pointer = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                    }
                    else
                    {
                        Object.Destroy(pointer);
                        pointer = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                    }

                    int bufferSize = (int)(width * height * 4);
                    byte[] imageBuffer = new byte[bufferSize];

                    if (SteamUtils.GetImageRGBA(imageHandle, imageBuffer, bufferSize))
                    {
                        pointer.LoadRawTextureData(Utilities.FlipImageBufferVertical((int)width, (int)height, imageBuffer));
                        pointer.Apply();
                    }

                    _loadedImages[imageHandle] = pointer;

                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Functions to allow game servers to set stats and achievements on players.
        /// </summary>
        public static class Server
        {
            private static CallResult<GSStatsReceived_t> _mGsStatsReceivedT;
            private static CallResult<GSStatsStored_t> _mGsStatsStoredT;

            /// <summary>
            /// Resets the specified user's achievement to an uncompleted status.
            /// </summary>
            /// <param name="userId">The Steam ID of the user whose achievement is to be cleared.</param>
            /// <param name="achievementApiName">The API name of the achievement to reset.</param>
            /// <returns>True if the achievement was successfully cleared; otherwise, false.</returns>
            public static bool ClearUserAchievement(CSteamID userId, string achievementApiName) =>
                SteamGameServerStats.ClearUserAchievement(userId, achievementApiName);

            /// <summary>
            /// Retrieves the unlocked status of a specific achievement for a given user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user for whom the achievement status is being retrieved.</param>
            /// <param name="achievementApiName">The API name of the achievement to check.</param>
            /// <param name="achieved">Outputs the unlocked status of the specified achievement. True if the achievement is unlocked; otherwise, false.</param>
            /// <returns>True if the operation was successful; otherwise, false. Ensure that user stats have been requested and that the achievement exists in the App Admin configuration on Steamworks.</returns>
            public static bool GetUserAchievement(CSteamID userId, string achievementApiName, out bool achieved) =>
                SteamGameServerStats.GetUserAchievement(userId, achievementApiName, out achieved);

            /// <summary>
            /// Retrieves the current value of a stat for a specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user whose stat is being queried.</param>
            /// <param name="statApiName">The API name of the stat to retrieve. It must adhere to the defined name length constraints.</param>
            /// <param name="data">The output variable where the retrieved stat value will be stored.</param>
            /// <returns>
            /// True if the stat was successfully retrieved. The following conditions must be met for success:
            /// - The stat exists and is properly configured in the Steamworks App Admin.
            /// - The RequestUserStats call has successfully completed.
            /// - The type of the variable provided matches the type defined for the stat in Steamworks.
            /// Returns false if any condition is unmet.
            /// </returns>
            public static bool GetUserStat(CSteamID userId, string statApiName, out int data) =>
                SteamGameServerStats.GetUserStat(userId, statApiName, out data);

            /// <summary>
            /// Retrieves the current value of a statistic for the specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user whose statistic value is to be retrieved.</param>
            /// <param name="statApiName">The API name of the statistic to retrieve. The name must conform to the limits defined in the Steamworks configuration.</param>
            /// <param name="data">Outputs the retrieved statistic value.</param>
            /// <returns>
            /// True if the statistic value was successfully retrieved. This requires the statistic to exist,
            /// the RequestUserStats call to have completed successfully, and the statistic's type to match the definition in the Steamworks settings. Otherwise, false.
            /// </returns>
            public static bool GetUserStat(CSteamID userId, string statApiName, out float data) =>
                SteamGameServerStats.GetUserStat(userId, statApiName, out data);

            /// <summary>
            /// Asynchronously downloads stats and achievements for the specified user from the server.
            /// </summary>
            /// <remarks>
            /// These stats will only be auto-updated for clients currently playing on the server. For other users you'll need to call this function again to refresh any data.
            /// </remarks>
            /// <param name="userId"></param>
            /// <param name="callback"></param>
            public static void RequestUserStats(CSteamID userId, Action<GSStatsReceived_t, bool> callback)
            {
                if (callback == null)
                    return;

                _mGsStatsReceivedT ??= CallResult<GSStatsReceived_t>.Create();

                var handle = SteamGameServerStats.RequestUserStats(userId);
                _mGsStatsReceivedT.Set(handle, callback.Invoke);
            }

            /// <summary>
            /// Unlocks an achievement for a specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user for whom the achievement will be unlocked.</param>
            /// <param name="achievementApiName">The API name of the achievement to be unlocked.</param>
            /// <returns>True if the achievement was successfully unlocked, provided all required conditions are met; otherwise, false.</returns>
            public static bool SetUserAchievement(CSteamID userId, string achievementApiName) =>
                SteamGameServerStats.SetUserAchievement(userId, achievementApiName);

            /// <summary>
            /// Sets the value of a specified statistic for a given user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user whose statistic is to be updated.</param>
            /// <param name="statApiName">The API name of the statistic to set.</param>
            /// <param name="data">The new value to assign to the statistic.</param>
            /// <returns>True if the statistic was successfully updated; otherwise, false.</returns>
            public static bool SetUserStat(CSteamID userId, string statApiName, int data) =>
                SteamGameServerStats.SetUserStat(userId, statApiName, data);

            /// <summary>
            /// Sets a statistical value for the specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user for whom the statistic is being set.</param>
            /// <param name="statApiName">The API name of the statistic to update.</param>
            /// <param name="data">The new value to assign to the statistic.</param>
            /// <returns>True if the statistic was successfully updated; otherwise, false.</returns>
            public static bool SetUserStat(CSteamID userId, string statApiName, float data) =>
                SteamGameServerStats.SetUserStat(userId, statApiName, data);

            /// <summary>
            /// Sends updated stats and achievements data to the server for permanent storage for the specified user.
            /// </summary>
            /// <param name="userId">The unique identifier of the user whose stats and achievements data will be stored.</param>
            /// <param name="callback">A callback function invoked when the operation completes, providing the result of the stats storage operation and a boolean indicating success.</param>
            public static void StoreUserStats(CSteamID userId, Action<GSStatsStored_t, bool> callback)
            {
                if (callback == null)
                    return;

                _mGsStatsStoredT ??= CallResult<GSStatsStored_t>.Create();

                var handle = SteamGameServerStats.StoreUserStats(userId);
                _mGsStatsStoredT.Set(handle, callback.Invoke);
            }

            /// <summary>
            /// Updates an AVGRATE stat with new values for the specified user.
            /// </summary>
            /// <param name="userId">The Steam ID of the user to update the AVGRATE stat for.</param>
            /// <param name="statApiName">The 'API Name' of the stat. Must not be longer than <see cref="Constants.k_cchStatNameMax"/>.</param>
            /// <param name="count">The value accumulation since the last call to this function.</param>
            /// <param name="length">The amount of time in seconds since the last call to this function.</param>
            /// <returns></returns>
            public static bool UpdateUserAvgRateStat(CSteamID userId, string statApiName, float count, double length) => SteamGameServerStats.UpdateUserAvgRateStat(userId, statApiName, count, length);
        }
    }

}
#endif