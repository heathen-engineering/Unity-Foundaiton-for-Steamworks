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
#if !DISABLESTEAMWORKS  && STEAM_INSTALLED
using Steamworks;
using System;
using System.ComponentModel;
using System.Threading;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam leaderboard, providing access to its metadata, entries, and associated operations.
    /// </summary>
    [Serializable]
    public struct LeaderboardData : IEquatable<SteamLeaderboard_t>, IEquatable<ulong>, IEquatable<string>
    {
        /// <summary>
        /// The unique identifier or name of the leaderboard.
        /// This value must match the name of the leaderboard as defined in Steamworks if it is not to be created at runtime.
        /// </summary>
        public string apiName;

        /// <summary>
        /// Represents the unique identifier for a leaderboard within Steamworks.
        /// This value can be null, indicating that no leaderboard has been associated.
        /// </summary>
        public SteamLeaderboard_t id;

        /// <summary>
        /// The display name of the leaderboard associated with this data.
        /// This value is retrieved from Steamworks using the leaderboard's unique identifier.
        /// </summary>
        public readonly string DisplayName => SteamUserStats.GetLeaderboardName(id);

        /// <summary>
        /// Indicates whether the leaderboard data is valid.
        /// A leaderboard is considered valid if its underlying SteamLeaderboard_t identifier has a value greater than 0.
        /// </summary>
        public readonly bool IsValid => id.m_SteamLeaderboard > 0;

        /// <summary>
        /// Represents the total number of entries in the associated leaderboard.
        /// This value is retrieved using the Steamworks API and reflects the current count of entries maintained by the leaderboard.
        /// </summary>
        public readonly int EntryCount => API.Leaderboards.Client.GetEntryCount(id);

        /// <summary>
        /// Retrieves the user entry for the local user from the specified leaderboard.
        /// </summary>
        /// <param name="maxDetailEntries">The maximum number of detail entries to retrieve for the user's leaderboard entry.</param>
        /// <param name="callback">The delegate to invoke upon completion, providing the retrieved leaderboard entry and an error state if applicable.</param>
        public readonly void GetUserEntry(int maxDetailEntries, Action<LeaderboardEntry, bool> callback)
        {
            API.Leaderboards.Client.DownloadEntries(id, new CSteamID[] { UserData.Me }, maxDetailEntries, (results, error) =>
            {
                if (error || results.Length == 0)
                    callback.Invoke(null, error);
                else
                    callback.Invoke(results[0], false);
            });
        }

        /// <summary>
        /// Retrieves the top entries from the leaderboard.
        /// </summary>
        /// <param name="count">The number of top entries to retrieve.</param>
        /// <param name="maxDetailEntries">The maximum number of detail entries to retrieve for each leaderboard entry.</param>
        /// <param name="callback">The delegate to invoke upon completion, providing the retrieved leaderboard entries and a boolean indicating success or failure.</param>
        public readonly void
            GetTopEntries(int count, int maxDetailEntries, Action<LeaderboardEntry[], bool> callback) =>
            GetEntries(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, count, maxDetailEntries, callback);

        /// <summary>
        /// Retrieves leaderboard entries within the specified range and invokes the callback with the results.
        /// </summary>
        /// <param name="request">Specifies the type of range to retrieve, such as global, around the user, or friends-only entries.</param>
        /// <param name="start">The starting index of the leaderboard entries to download.</param>
        /// <param name="end">The ending index of the leaderboard entries to download.</param>
        /// <param name="maxDetailEntries">The maximum number of detailed values to retrieve per leaderboard entry.</param>
        /// <param name="callback">The delegate invoked upon completion, providing an array of retrieved leaderboard entries and the success state of the operation.</param>
        public readonly void GetEntries(ELeaderboardDataRequest request, int start, int end, int maxDetailEntries,
            Action<LeaderboardEntry[], bool> callback) =>
            API.Leaderboards.Client.DownloadEntries(id, request, start, end, maxDetailEntries, callback);

        /// <summary>
        /// Retrieves leaderboard entries for the specified users and invokes the provided callback upon completion.
        /// </summary>
        /// <param name="users">An array of users for whom the leaderboard entries are to be retrieved.</param>
        /// <param name="maxDetailEntries">The maximum number of detail entries to retrieve for each user's leaderboard entry.</param>
        /// <param name="callback">The delegate to invoke when the retrieval process is completed, providing an array of leaderboard entries and a success state.</param>
        public readonly void GetEntries(UserData[] users, int maxDetailEntries,
            Action<LeaderboardEntry[], bool> callback) => API.Leaderboards.Client.DownloadEntries(id,
            Array.ConvertAll(users, p => p.id), maxDetailEntries, callback);

        /// <summary>
        /// Retrieves all leaderboard entries for the specified leaderboard.
        /// This operation may not return all results as Steam could impose rate limits on the request.
        /// </summary>
        /// <param name="maxDetailEntries">The maximum number of detail entries to retrieve for each leaderboard entry.</param>
        /// <param name="callback">The delegate to invoke upon completion, providing the retrieved leaderboard entries and a success state.</param>
        public readonly void GetAllEntries(int maxDetailEntries, Action<LeaderboardEntry[], bool> callback)
        {
            GetEntries(ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 0, int.MaxValue, maxDetailEntries,
                callback);
        }

        /// <summary>
        /// Retrieves leaderboard entries for the specified users.
        /// </summary>
        /// <param name="users">The array of user SteamIDs for whom leaderboard entries will be retrieved.</param>
        /// <param name="maxDetailEntries">The maximum number of detail entries to retrieve for each user's leaderboard entry.</param>
        /// <param name="callback">The delegate to invoke upon completion, providing the retrieved leaderboard entries and a success state.</param>
        public readonly void GetEntries(CSteamID[] users, int maxDetailEntries,
            Action<LeaderboardEntry[], bool> callback) =>
            API.Leaderboards.Client.DownloadEntries(id, users, maxDetailEntries, callback);

        /// <summary>
        /// Retrieves the leaderboard data corresponding to the specified board name.
        /// </summary>
        /// <param name="name">The name of the leaderboard to retrieve, as defined in the API.</param>
        /// <param name="callback">The delegate to invoke when the retrieval process completes, providing the retrieved <see cref="LeaderboardData"/> and a boolean indicating whether an I/O error occurred.</param>
        public static void Get(string name, Action<LeaderboardData, bool> callback) =>
            API.Leaderboards.Client.Find(name, callback);

        /// <summary>
        /// Retrieves the leaderboard data associated with the specified ID.
        /// </summary>
        /// <param name="id">The unique identifier of the leaderboard to retrieve.</param>
        /// <returns>An instance of <see cref="LeaderboardData"/> representing the requested leaderboard.</returns>
        public static LeaderboardData Get(ulong id) => id;

        /// <summary>
        /// Retrieves the leaderboard data associated with the specified leaderboard ID.
        /// </summary>
        /// <param name="id">The unique identifier of the leaderboard to retrieve.</param>
        /// <returns>An instance of <see cref="LeaderboardData"/> representing the leaderboard details.</returns>
        public static LeaderboardData Get(SteamLeaderboard_t id) => id;

        /// <summary>
        /// Represents a request structure used to fetch or create Steam leaderboards,
        /// including settings such as name, sort method, display type, and creation flag.
        /// </summary>
        [Serializable]
        public struct GetAllRequest
        {
            public bool create;
            public string name;
            public ELeaderboardDisplayType type;
            public ELeaderboardSortMethod sort;
        }

        /// <summary>
        /// Retrieves all leaderboard data as specified by the provided input commands.
        /// </summary>
        /// <param name="commands">An array of requests specifying the leaderboards to fetch.</param>
        /// <param name="callback">A delegate to invoke upon completion, providing the retrieved leaderboard data and the result state.</param>
        public static void GetAll(GetAllRequest[] commands, Action<LeaderboardData[], EResult> callback)
        {
            if (commands == null || commands.Length == 0)
            {
                callback?.Invoke(null, EResult.k_EResultOK);
                return;
            }

            var boards = new LeaderboardData[commands.Length];

            try
            {
                var bgWorker = new BackgroundWorker();
                bgWorker.DoWork += BgWorker_DoWork;
                bgWorker.RunWorkerCompleted += (_, arguments) =>
                {
                    if (arguments.Cancelled)
                        callback?.Invoke(null, EResult.k_EResultCancelled);
                    else if (arguments.Error != null)
                        callback?.Invoke(null, EResult.k_EResultUnexpectedError);
                    else
                    {
                        if (arguments.Result is LeaderboardData[] results)
                            for (int i = 0; i < results.Length; i++)
                            {
                                boards[i] = results[i];
                            }

                        callback?.Invoke(boards, EResult.k_EResultOK);
                    }

                    bgWorker.Dispose();
                };
                bgWorker.RunWorkerAsync(commands);
            }
            catch (Exception ex)
            {
                Debug.LogError("Get All Leaderboards experienced and unhandled exception: " + ex);
                callback?.Invoke(null, EResult.k_EResultUnexpectedError);
            }
        }

        private static void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is not GetAllRequest[] boards) return;
            var results = new LeaderboardData[boards.Length];

            for (var i = 0; i < boards.Length; i++)
            {
                try
                {
                    var board = boards[i];
                    var waiting = true;
                    if (board.create)
                    {
                        var i1 = i;
                        GetOrCreate(board.name, board.type, board.sort, (result, _) =>
                        {
                            results[i1] = result;
                            waiting = false;
                        });
                    }
                    else
                    {
                        var i1 = i;
                        Get(board.name, (result, _) =>
                        {
                            results[i1] = result;
                            waiting = false;
                        });
                    }

                    while (waiting)
                    {
                        Thread.Sleep(10);
                    }
                }
                catch
                {
                    results[i] = default;
                }
            }

            e.Result = results;
        }

        /// <summary>
        /// Finds or creates a leaderboard with the specified parameters.
        /// </summary>
        /// <param name="name">The API name of the leaderboard.</param>
        /// <param name="displayType">The display type of the leaderboard, specifying how its entries should be presented.</param>
        /// <param name="sortMethod">The sorting method for the leaderboard's entries.</param>
        /// <param name="callback">A delegate to invoke upon completion, providing the resulting leaderboard data and an indication of whether there was an I/O error.</param>
        public static void GetOrCreate(string name, ELeaderboardDisplayType displayType,
            ELeaderboardSortMethod sortMethod, Action<LeaderboardData, bool> callback) =>
            API.Leaderboards.Client.FindOrCreate(name, sortMethod, displayType, callback);

        /// <summary>
        /// Uploads a score to the leaderboard using the "keep best" upload mode. The leaderboard will keep the highest score between the existing score and the uploaded score.
        /// </summary>
        /// <param name="score">The score to be uploaded to the leaderboard.</param>
        /// <param name="callback">Optional callback invoked upon completion, providing the result of the upload and a success state.</param>
        public readonly void UploadScoreKeepBest(int score, Action<LeaderboardScoreUploaded, bool> callback = null) =>
            UploadScore(score, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, callback);

        /// <summary>
        /// Uploads a score to the leaderboard using the force update method, ensuring that the submitted score replaces any existing score.
        /// </summary>
        /// <param name="score">The score value to upload to the leaderboard.</param>
        /// <param name="callback">
        /// The delegate to invoke upon completion of the upload. This provides information about the upload result via a
        /// <see cref="LeaderboardScoreUploaded"/> object and a success flag.
        /// </param>
        public readonly void UploadScoreForceUpdate(int score, Action<LeaderboardScoreUploaded, bool> callback = null) => UploadScore(score, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, callback);

        /// <summary>
        /// Uploads a score to the leaderboard using the "Keep Best" upload method, where only the best score is retained.
        /// </summary>
        /// <param name="score">The score to upload to the leaderboard.</param>
        /// <param name="details">An array of integers containing additional details about the score.</param>
        /// <param name="callback">The delegate to invoke upon completion, providing details of the score upload and a success state.</param>
        public readonly void UploadScoreKeepBest(int score, int[] details, Action<LeaderboardScoreUploaded, bool> callback = null) => UploadScore(score, details, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, callback);

        /// <summary>
        /// Uploads a score to the leaderboard using the Force Update method, which overrides any existing scores for the current user.
        /// </summary>
        /// <param name="score">The score to upload to the leaderboard.</param>
        /// <param name="details">An optional array of additional data to attach to the score entry.</param>
        /// <param name="callback">The delegate to invoke upon completion, providing details of the score upload operation and a success indicator.</param>
        public readonly void UploadScoreForceUpdate(int score, int[] details, Action<LeaderboardScoreUploaded, bool> callback = null) => UploadScore(score, details, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, callback);

        /// <summary>
        /// Uploads a score for the player to the leaderboard.
        /// </summary>
        /// <param name="score">The score to upload to the leaderboard.</param>
        /// <param name="method">The method used to upload the score, such as keeping the best score or forcing an update.</param>
        /// <param name="callback">A delegate invoked upon completion of the upload process, providing the results of the score upload and an error state if applicable.</param>
        public readonly void UploadScore(int score, ELeaderboardUploadScoreMethod method,
            Action<LeaderboardScoreUploaded, bool> callback = null) =>
            API.Leaderboards.Client.UploadScore(id, method, score, null, callback);

        /// <summary>
        /// Uploads a score for the player to the leaderboard.
        /// </summary>
        /// <param name="score">The score to upload to the leaderboard.</param>
        /// <param name="scoreDetails">Optional detailed breakdown of the score.</param>
        /// <param name="method">The method used to upload the score, such as keeping the best score or forcing an update.</param>
        /// <param name="callback">Optional delegate to invoke upon the completion of the score upload. Provides the result of the upload process and an error state if applicable.</param>
        public readonly void UploadScore(int score, int[] scoreDetails, ELeaderboardUploadScoreMethod method,
            Action<LeaderboardScoreUploaded, bool> callback = null) =>
            API.Leaderboards.Client.UploadScore(id, method, score, scoreDetails, callback);

        /// <summary>
        /// Forces the upload of a score to the leaderboard, overriding any existing score.
        /// </summary>
        /// <param name="score">The score to upload as a string. It will be parsed into an integer before uploading.</param>
        public readonly void ForceUploadScore(string score)
        {
            if (int.TryParse(score, out int result))
                UploadScore(result, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate);
        }

        /// <summary>
        /// Forces the upload of a specified score to the leaderboard using the ForceUpdate method.
        /// </summary>
        /// <param name="score">The score to upload to the leaderboard.</param>
        public readonly void ForceUploadScore(int score) => UploadScore(score,
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate);

        /// <summary>
        /// Uploads a score to the leaderboard using the ForceUpdate method, ensuring the submitted score is updated on the leaderboard regardless of any conditions.
        /// </summary>
        /// <param name="score">The score to be uploaded to the leaderboard.</param>
        /// <param name="details">Additional details or score components to be attached to the leaderboard entry.</param>
        public readonly void ForceUploadScore(int score, int[] details) => UploadScore(score, details,
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate);

        /// <summary>
        /// Uploads a score to the leaderboard using the "Keep Best" method, retaining the higher of the current score or the newly provided score.
        /// </summary>
        /// <param name="score">The score to upload as a string. If the string cannot be converted to an integer, the score will not be uploaded.</param>
        public readonly void KeepBestUploadScore(string score)
        {
            if (int.TryParse(score, out int result))
                UploadScore(result, ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest);
        }

        /// <summary>
        /// Uploads a score to the leaderboard using the "Keep Best" method.
        /// </summary>
        /// <param name="score">The score to be uploaded that will be compared with the existing score, retaining the best value.</param>
        public readonly void KeepBestUploadScore(int score) => UploadScore(score,
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest);

        /// <summary>
        /// Uploads the specified score to the leaderboard using the keep-best upload method.
        /// </summary>
        /// <param name="score">The score to upload to the leaderboard.</param>
        /// <param name="details">An array of additional details to be associated with the leaderboard entry.</param>
        public readonly void KeepBestUploadScore(int score, int[] details) => UploadScore(score, details,
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest);

        #region Boilerplate

        public readonly override string ToString()
        {
            return apiName;
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode() + apiName.GetHashCode();
        }

        public readonly override bool Equals(object obj)
        {
            if (obj is SteamLeaderboard_t t)
                return Equals(t);
            else if (obj is string s)
                return Equals(s);
            else if (obj is ulong ulongValue)
                return Equals(ulongValue);
            else
                return id.Equals(obj);
        }

        public readonly bool Equals(SteamLeaderboard_t other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(ulong other)
        {
            return id.m_SteamLeaderboard.Equals(other);
        }

        public readonly bool Equals(string other)
        {
            return apiName.Equals(other);
        }

        public static bool operator ==(LeaderboardData l, LeaderboardData r) => l.id == r.id;
        public static bool operator ==(LeaderboardData l, ulong r) => l.id.m_SteamLeaderboard == r;
        public static bool operator ==(LeaderboardData l, string r) => l.apiName == r;
        public static bool operator ==(LeaderboardData l, SteamLeaderboard_t r) => l.id == r;
        public static bool operator !=(LeaderboardData l, LeaderboardData r) => l.id != r.id;
        public static bool operator !=(LeaderboardData l, ulong r) => l.id.m_SteamLeaderboard != r;
        public static bool operator !=(LeaderboardData l, string r) => l.apiName != r;
        public static bool operator !=(LeaderboardData l, SteamLeaderboard_t r) => l.id != r;

        public static implicit operator ulong(LeaderboardData c) => c.id.m_SteamLeaderboard;
        public static implicit operator LeaderboardData(ulong id) => new LeaderboardData { id = new SteamLeaderboard_t(id), apiName = API.Leaderboards.Client.GetName(new SteamLeaderboard_t(id)) };
        public static implicit operator SteamLeaderboard_t(LeaderboardData c) => c.id;
        public static implicit operator LeaderboardData(SteamLeaderboard_t id) => new LeaderboardData { id = id };
        public static implicit operator string(LeaderboardData c) => c.apiName;
        #endregion
    }
}
#endif
