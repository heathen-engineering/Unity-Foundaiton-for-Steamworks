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
using System.ComponentModel;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// A static class providing functionality for interacting with Steam leaderboards using the Heathen Steamworks Integration framework.
    /// </summary>
    /// <remarks>
    /// The Leaderboards class offers an interface to perform operations such as finding leaderboards,
    /// creating new leaderboards, retrieving entries, uploading scores, and attaching UGC (User-Generated Content).
    /// It is primarily used within the Steamworks ecosystem for managing leaderboard data.
    /// </remarks>
    public static class Leaderboards
    {
        private struct DownloadScoreRequest
        {
            public bool UserRequest;
            public CSteamID[] Users;
            public SteamLeaderboard_t Leaderboard;
            public ELeaderboardDataRequest Request;
            public int Start;
            public int End;
            public int MaxDetailsPerEntry;
            public Action<LeaderboardEntry[], bool> Callback;
        }

        private struct UploadScoreRequest
        {
            public LeaderboardData Leaderboard;
            public ELeaderboardUploadScoreMethod Method;
            public int Score;
            public int[] Details;
            public Action<LeaderboardScoreUploaded, bool> Callback;
        }

        private struct FindOrCreateRequest
        {
            public string APIName;
            public bool CreateIfMissing;
            public ELeaderboardDisplayType DisplayType;
            public ELeaderboardSortMethod SortMethod;
            public Action<LeaderboardData, bool> Callback;
        }

        /// <summary>
        /// A static class providing client-side utilities for interacting with leaderboards in the Heathen Steamworks Integration framework.
        /// </summary>
        /// <remarks>
        /// The Client class enables users to perform various operations on leaderboards, such as searching, creating, downloading entries,
        /// uploading scores, and attaching user-generated content (UGC). It facilitates communication with Steamworks APIs and provides access
        /// to events and properties for tracking leaderboard-related activities.
        /// </remarks>
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                _mLeaderboardScoresDownloadedT = null;
                _mLeaderboardFindResultT = null;
                _mLeaderboardScoreUploadedT = null;
                _downloadQueue = null;
                _uploadQueue = null;
                _findOrCreateQueue = null;
                RequestTimeout = 30f;

                OnScoreUploaded = new();
            }

            /// <summary>
            /// Event invoked when a score has been uploaded to a leaderboard.
            /// </summary>
            /// <remarks>
            /// The event provides the result of the upload process, including details of the uploaded score,
            /// and a boolean indicating whether the operation was successful.
            /// </remarks>
            public static UnityEvent<LeaderboardScoreUploaded, bool> OnScoreUploaded = new();

            private static CallResult<LeaderboardScoresDownloaded_t> _mLeaderboardScoresDownloadedT;
            private static CallResult<LeaderboardFindResult_t> _mLeaderboardFindResultT;
            private static CallResult<LeaderboardScoreUploaded_t> _mLeaderboardScoreUploadedT;

            private static Queue<DownloadScoreRequest> _downloadQueue;
            private static Queue<UploadScoreRequest> _uploadQueue;
            private static Queue<FindOrCreateRequest> _findOrCreateQueue;

            private static void ExecuteDownloadRequest()
            {
                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (_, _) =>
                {
                    bool waiting = false;

                    while (_downloadQueue.Count > 0)
                    {
                        //Check on the next request
                        var request = _downloadQueue.Peek();

                        //If the request is still valid
                        if (request.Callback != null)
                        {
                            //Initiate the request with Valve
                            if (request.UserRequest)
                            {
                                var handle = SteamUserStats.DownloadLeaderboardEntriesForUsers(request.Leaderboard, request.Users, request.Users.Length);

                                //Set our waiting and time out
                                waiting = true;

                                _mLeaderboardScoresDownloadedT.Set(handle, (results, error) =>
                                {
                                    request.Callback.Invoke(ProcessScoresDownloaded(results, error, request.MaxDetailsPerEntry), error);
                                    waiting = false;
                                });

                                //Wait until we get a result
                                while (waiting)
                                    Thread.Sleep(100);
                            }
                            else
                            {
                                var handle = SteamUserStats.DownloadLeaderboardEntries(request.Leaderboard, request.Request, request.Start, request.End);

                                //Set our waiting and time out
                                waiting = true;

                                //Set our call result handler
                                _mLeaderboardScoresDownloadedT.Set(handle, (results, error) =>
                                {
                                    request.Callback.Invoke(ProcessScoresDownloaded(results, error, request.MaxDetailsPerEntry), error);
                                    waiting = false;
                                });

                                //Wait until we get a result
                                while (waiting)
                                    Thread.Sleep(100);
                            }
                        }

                        _downloadQueue.Dequeue();
                    }
                };
                bgWorker.RunWorkerCompleted += (_, _) =>
                {
                    bgWorker.Dispose();
                };
                bgWorker.RunWorkerAsync();
            }

            private static void ExecuteUploadRequest()
            {
                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (_, _) =>
                {
                    bool waiting = false;

                    while (_uploadQueue.Count > 0)
                    {
                        //Check on the next request
                        var request = _uploadQueue.Peek();

                        //If the request is still valid

                        //Initiate the request with Valve
                        var handle = SteamUserStats.UploadLeaderboardScore(request.Leaderboard, request.Method, request.Score, request.Details, request.Details?.Length ?? 0);

                        waiting = true;

                        _mLeaderboardScoreUploadedT.Set(handle, (r, e) =>
                        {
                            OnScoreUploaded?.Invoke(r, e);
                            request.Callback?.Invoke(r, e);
                            waiting = false;
                        });

                        //Wait until we get a result
                        while (waiting)
                            Thread.Sleep(100);

                        if (waiting)
                        {
                            request.Callback?.Invoke(default, true);
                            Debug.LogWarning("Leaderboard upload request exceeded the timeout of " + RequestTimeout + ", the callback will be called as a failure and next request serviced. The request may still come in at a later time.");
                        }

                        _uploadQueue.Dequeue();
                    }
                };
                bgWorker.RunWorkerCompleted += (_, _) =>
                    {
                        bgWorker.Dispose();
                    };
                bgWorker.RunWorkerAsync();
            }

            private static void ExecuteFindOrCreateRequest()
            {
                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += (_, _) =>
                {
                    bool waiting = false;

                    while (_findOrCreateQueue.Count > 0)
                    {
                        //Check on the next request
                        var request = _findOrCreateQueue.Peek();

                        //If the request is still valid
                        if (request.Callback != null)
                        {
                            //Initiate the request with Valve
                            if (request.CreateIfMissing)
                            {
                                var handle = SteamUserStats.FindOrCreateLeaderboard(request.APIName, request.SortMethod, request.DisplayType);

                                //Set our waiting and time out
                                waiting = true;

                                _mLeaderboardFindResultT.Set(handle, (results, error) =>
                                {
                                    var board = new LeaderboardData
                                    {
                                        apiName = request.APIName,
                                        id = results.m_hSteamLeaderboard
                                    };
                                    SteamTools.Interface.AddBoard(board);
                                    request.Callback(board,
                                    error);
                                    waiting = false;
                                });

                                //Wait until we get a result
                                while (waiting)
                                    Thread.Sleep(10);
                            }
                            else
                            {
                                var handle = SteamUserStats.FindLeaderboard(request.APIName);

                                //Set our waiting and time out
                                waiting = true;

                                //Set our call result handler
                                _mLeaderboardFindResultT.Set(handle, (results, error) =>
                                {
                                    var board = new LeaderboardData
                                    {
                                        apiName = request.APIName,
                                        id = results.m_hSteamLeaderboard
                                    };
                                    SteamTools.Interface.AddBoard(board);

                                    request.Callback(board,
                                    error);
                                    waiting = false;
                                });

                                //Wait until we get a result
                                while (waiting)
                                    Thread.Sleep(10);
                            }
                        }

                        _findOrCreateQueue.Dequeue();
                    }
                };
                bgWorker.RunWorkerCompleted += (_, _) =>
                {
                    bgWorker.Dispose();
                };
                bgWorker.RunWorkerAsync();
            }
            
            /// <summary>
            /// The amount of time to wait for Valve to respond to Leaderboard requests.
            /// </summary>
            public static float RequestTimeout { get; set; } = 30f;
            /// <summary>
            /// The number of pending requests to download scores from a leaderboard
            /// </summary>
            public static int PendingDownloadScoreRequests => _downloadQueue?.Count ?? 0;
            /// <summary>
            /// The number of pending requests to upload scores to a leaderboard for the local user
            /// </summary>
            public static int PendingUploadScoreRequests => _uploadQueue?.Count ?? 0;

            /// <summary>
            /// Gets the number of pending requests to find or create a leaderboard.
            /// </summary>
            /// <remarks>
            /// This property represents the count of requests currently queued for processing
            /// in the system to either locate an existing leaderboard or create a new one.
            /// A non-zero value indicates that there are active operations waiting to be handled.
            /// </remarks>
            public static int PendingFindOrCreateRequests => _findOrCreateQueue?.Count ?? 0;

            /// <summary>
            /// Downloads entries from a specified leaderboard.
            /// </summary>
            /// <remarks>
            /// This method retrieves leaderboard entries, allowing you to define the scope of the entries to fetch.
            /// The results can include user scores and optional metadata details per entry. This is useful for displaying
            /// leaderboard rankings or accessing user-specific data for external processing.
            /// </remarks>
            /// <param name="leaderboard">The leaderboard from which entries will be downloaded. This is identified by the leaderboard data structure.</param>
            /// <param name="request">Specifies the type of data to request, such as global scores or scores relative to the user.</param>
            /// <param name="start">The starting index of the entries to download. This index is inclusive.</param>
            /// <param name="end">The ending index of the entries to download. This index is inclusive.</param>
            /// <param name="maxDetailsPerEntry">The maximum number of metadata details to retrieve for each leaderboard entry. Set to 0 if details are not needed.</param>
            /// <param name="callback">The action to invoke once the entries are downloaded. The callback provides an array of leaderboard entries and a boolean indicating success or failure.</param>
            public static void DownloadEntries(LeaderboardData leaderboard, ELeaderboardDataRequest request, int start,
                int end, int maxDetailsPerEntry, Action<LeaderboardEntry[], bool> callback)
            {
                if (callback == null)
                    return;

                _mLeaderboardScoresDownloadedT ??= CallResult<LeaderboardScoresDownloaded_t>.Create();


                _downloadQueue ??= new Queue<DownloadScoreRequest>();

                var nRequest = new DownloadScoreRequest
                {
                    UserRequest = false,
                    Leaderboard = leaderboard,
                    Request = request,
                    Start = start,
                    End = end,
                    MaxDetailsPerEntry = maxDetailsPerEntry,
                    Callback = callback
                };

                _downloadQueue.Enqueue(nRequest);

                //If we only have 1 enqueued, then we need to start the executing if we have more than it's already running
                if (_downloadQueue.Count == 1)
                    ExecuteDownloadRequest();
            }

            /// <summary>
            /// Downloads entries from a leaderboard for a specified set of users.
            /// </summary>
            /// <remarks>
            /// This method retrieves leaderboard entries for the given users, including an optional number of additional details per entry.
            /// A callback is invoked upon completion with the retrieved entries and a bool indicating success or failure.
            /// </remarks>
            /// <param name="leaderboard">The leaderboard from which to fetch entries, represented by a LeaderboardData object.</param>
            /// <param name="users">An array of user Steam IDs for whom the leaderboard entries are to be downloaded.</param>
            /// <param name="maxDetailsPerEntry">The maximum number of additional details to retrieve per leaderboard entry.</param>
            /// <param name="callback">
            /// The function to invoke after entries are downloaded. It provides an array of <see cref="LeaderboardEntry"/> objects and a boolean indicating success or failure.
            /// </param>
            public static void DownloadEntries(LeaderboardData leaderboard, CSteamID[] users, int maxDetailsPerEntry,
                Action<LeaderboardEntry[], bool> callback)
            {
                if (callback == null)
                    return;

                _mLeaderboardScoresDownloadedT ??= CallResult<LeaderboardScoresDownloaded_t>.Create();

                _downloadQueue ??= new Queue<DownloadScoreRequest>();

                var nRequest = new DownloadScoreRequest
                {
                    UserRequest = true,
                    Leaderboard = leaderboard,
                    Users = users,
                    MaxDetailsPerEntry = maxDetailsPerEntry,
                    Callback = callback
                };

                _downloadQueue.Enqueue(nRequest);

                //If we only have 1 enqueued, then we need to start the executing if we have more than it's already running
                if (_downloadQueue.Count == 1)
                    ExecuteDownloadRequest();
            }

            /// <summary>
            /// Downloads leaderboard entries for the given leaderboard and users.
            /// </summary>
            /// <remarks>
            /// Retrieves the entries associated with a specific leaderboard and the provided user list. This includes score details and other relevant information.
            /// </remarks>
            /// <param name="leaderboard">The leaderboard from which entries will be downloaded.</param>
            /// <param name="users">An array of users whose leaderboard entries are to be retrieved.</param>
            /// <param name="maxDetailsPerEntry">The maximum number of additional data details to include for each entry.</param>
            /// <param name="callback">The callback to invoke when the request is completed. It provides an array of retrieved leaderboard entries and a boolean indicating success or failure.</param>
            public static void DownloadEntries(LeaderboardData leaderboard, UserData[] users, int maxDetailsPerEntry,
                Action<LeaderboardEntry[], bool> callback) => DownloadEntries(leaderboard,
                Array.ConvertAll(users, (i) => i.id), maxDetailsPerEntry, callback);
            /// <summary>
            /// Gets a leaderboard by name.
            /// </summary>
            /// <param name="leaderboardName">The name of the leaderboard to find. Must not be longer than <see cref="Constants.k_cchLeaderboardNameMax"/>.</param>
            /// <param name="callback"></param>
            public static void Find(string leaderboardName, Action<LeaderboardData, bool> callback)
            {
                if (callback == null)
                    return;

                _mLeaderboardFindResultT ??= CallResult<LeaderboardFindResult_t>.Create();

                _findOrCreateQueue ??= new Queue<FindOrCreateRequest>();

                _findOrCreateQueue.Enqueue(new FindOrCreateRequest
                {
                    APIName = leaderboardName,
                    Callback = callback,
                    CreateIfMissing = false
                });

                if (_findOrCreateQueue.Count == 1)
                    ExecuteFindOrCreateRequest();
            }
            /// <summary>
            /// Gets a leaderboard by name, it will create it if it's not yet created.
            /// </summary>
            /// <remarks>
            /// Leaderboards created with this function will not automatically show up in the Steam Community. You must manually set the Community Name field in the App Admin panel of the Steamworks website. As such, it's generally recommended to prefer creating the leaderboards in the App Admin panel on the Steamworks website and using FindLeaderboard unless you're expected to have a large number of dynamically created leaderboards.
            /// </remarks>
            /// <param name="leaderboardName">The name of the leaderboard to find. Must not be longer than <see cref="Constants.k_cchLeaderboardNameMax"/>.</param>
            /// <param name="sortingMethod">The sort order of the new leaderboard if it's created.</param>
            /// <param name="displayType">The display type (used by the Steam Community website) of the new leaderboard if it's created.</param>
            /// <param name="callback"></param>
            public static void FindOrCreate(string leaderboardName, ELeaderboardSortMethod sortingMethod, ELeaderboardDisplayType displayType, Action<LeaderboardData, bool> callback)
            {
                if (callback == null)
                    return;

                _mLeaderboardFindResultT ??= CallResult<LeaderboardFindResult_t>.Create();

                if (sortingMethod == ELeaderboardSortMethod.k_ELeaderboardSortMethodNone)
                {
                    Debug.LogError("You should never pass ELeaderboardSortMethod.k_ELeaderboardSortMethodNone for the sorting method as this is undefined behaviour.");
                    callback(default, true);
                    return;
                }

                if (displayType == ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNone)
                {
                    Debug.LogError("You should never pass ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNone for the display type as this is undefined behaviour.");
                    callback(default, true);
                    return;
                }

                _findOrCreateQueue ??= new Queue<FindOrCreateRequest>();

                _findOrCreateQueue.Enqueue(new FindOrCreateRequest
                {
                    APIName = leaderboardName,
                    Callback = callback,
                    CreateIfMissing = true,
                    SortMethod = sortingMethod,
                    DisplayType = displayType
                });

                if (_findOrCreateQueue.Count == 1)
                    ExecuteFindOrCreateRequest();
            }
            /// <summary>
            /// Returns the display type of the leaderboard handle.
            /// </summary>
            /// <param name="leaderboard"></param>
            /// <returns></returns>
            public static ELeaderboardDisplayType GetDisplayType(LeaderboardData leaderboard) => SteamUserStats.GetLeaderboardDisplayType(leaderboard);
            /// <summary>
            /// Returns the total number of entries in a leaderboard.
            /// </summary>
            /// <param name="leaderboard"></param>
            /// <returns></returns>
            public static int GetEntryCount(LeaderboardData leaderboard) => SteamUserStats.GetLeaderboardEntryCount(leaderboard);
            /// <summary>
            /// Returns the name of a leaderboard handle.
            /// </summary>
            /// <param name="leaderboard"></param>
            /// <returns></returns>
            public static string GetName(LeaderboardData leaderboard) => SteamUserStats.GetLeaderboardName(leaderboard);
            /// <summary>
            /// Returns the sort order of a leaderboard handle.
            /// </summary>
            /// <param name="leaderboard"></param>
            /// <returns></returns>
            public static ELeaderboardSortMethod GetSortMethod(LeaderboardData leaderboard) => SteamUserStats.GetLeaderboardSortMethod(leaderboard);

            /// <summary>
            /// Uploads a score to a specified leaderboard using the provided method and details.
            /// </summary>
            /// <remarks>
            /// Depending on the upload method specified, the score will be set, updated, or applied conditionally to the leaderboard.
            /// Additional details can be included with the score, and a callback can be provided to handle the result of the upload.
            /// </remarks>
            /// <param name="leaderboard">The target leaderboard to which the score will be uploaded.</param>
            /// <param name="method">The method used to upload the score, such as setting a new score, appending it, or conditional updating.</param>
            /// <param name="score">The score value to be uploaded to the leaderboard.</param>
            /// <param name="details">An array of additional details associated with the score. This can be used to store custom metadata.</param>
            /// <param name="callback">A callback function to handle the result of the score upload. It receives the upload result and a boolean indicating success.</param>
            public static void UploadScore(LeaderboardData leaderboard, ELeaderboardUploadScoreMethod method, int score,
                int[] details, Action<LeaderboardScoreUploaded, bool> callback = null)
            {
                _mLeaderboardScoreUploadedT ??= CallResult<LeaderboardScoreUploaded_t>.Create();

                _uploadQueue ??= new Queue<UploadScoreRequest>();

                var nRequest = new UploadScoreRequest
                {
                    Leaderboard = leaderboard,
                    Method = method,
                    Score = score,
                    Details = details,
                    Callback = callback,
                };

                _uploadQueue.Enqueue(nRequest);

                //If we only have 1 enqueued, then we need to start the executing if we have more than it's already running
                if (_uploadQueue.Count == 1)
                    ExecuteUploadRequest();
            }

            /// <summary>
            /// Processes downloaded leaderboard scores and retrieves leaderboard entries based on the specified parameters.
            /// </summary>
            /// <param name="param">The data structure containing the downloaded leaderboard scores and associated metadata.</param>
            /// <param name="bIOFailure">A boolean indicating whether an I/O failure occurred during the score download.</param>
            /// <param name="maxDetailEntries">The maximum number of detailed entries to process from the downloaded scores.</param>
            /// <returns>An array of <see cref="LeaderboardEntry"/> objects representing the processed leaderboard entries.</returns>
            private static LeaderboardEntry[] ProcessScoresDownloaded(LeaderboardScoresDownloaded_t param,
                bool bIOFailure, int maxDetailEntries)
            {
                //Check for the current users data in the record set and update accordingly
                if (!bIOFailure)
                {
                    var entries = new LeaderboardEntry[param.m_cEntryCount];

                    for (int i = 0; i < param.m_cEntryCount; i++)
                    {
                        LeaderboardEntry_t buffer;
                        int[] details = null;

                        if (maxDetailEntries < 1)
                            SteamUserStats.GetDownloadedLeaderboardEntry(param.m_hSteamLeaderboardEntries, i, out buffer, details, maxDetailEntries);
                        else
                        {
                            details = new int[maxDetailEntries];
                            SteamUserStats.GetDownloadedLeaderboardEntry(param.m_hSteamLeaderboardEntries, i, out buffer, details, maxDetailEntries);
                        }
                        
                        var record = new LeaderboardEntry
                        {
                            Entry = buffer,
                            Details = details
                        };

                        entries[i] = record;
                    }

                    return entries;
                }
                else
                    return Array.Empty<LeaderboardEntry>();
            }

        }
    }
}
#endif
