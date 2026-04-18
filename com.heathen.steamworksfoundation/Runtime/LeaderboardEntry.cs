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
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>Represents a single leaderboard entry, including optional UGC attachment support.</summary>
    public class LeaderboardEntry
    {
        public LeaderboardEntry_t Entry;
        public int[]   Details;
        public UserData User      => Entry.m_steamIDUser;
        public int      Rank      => Entry.m_nGlobalRank;
        public int      Score     => Entry.m_nScore;
        public UGCHandle_t UgcHandle => Entry.m_hUGC;
        public int this[int index] => Details[index];

        /// <summary>Cached file name from the last successful UGC download.</summary>
        public string CashedUgcFileName = string.Empty;
        public bool   HasCashedUgcFileName => !string.IsNullOrEmpty(CashedUgcFileName);

        /// <summary>Invoked when a UGC download completes, providing the file name (or null on failure).</summary>
        public UnityEvent<string> EvtUgcDownloaded = new();

        /// <summary>
        /// Downloads the UGC file attached to this entry and deserializes it as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">A JsonUtility-serialisable type.</typeparam>
        /// <param name="callback">Receives the result and a failure flag. If failure is true an error occurred.</param>
        public void GetAttachedUgc<T>(Action<T, bool> callback = null)
        {
            if (UgcHandle == UGCHandle_t.Invalid)
            {
                callback?.Invoke(default, true);
                return;
            }

            var call = SteamRemoteStorage.UGCDownload(UgcHandle, 0);
            var callResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create();
            callResult.Set(call, (result, bIOFailure) =>
            {
                if (!bIOFailure && result.m_eResult == EResult.k_EResultOK)
                {
                    CashedUgcFileName = result.m_pchFileName;
                    EvtUgcDownloaded.Invoke(result.m_pchFileName);

                    if (callback != null)
                    {
                        var bufferSize = result.m_nSizeInBytes;
                        var buffer = new byte[bufferSize];
                        SteamRemoteStorage.UGCRead(result.m_hFile, buffer, bufferSize, 0, EUGCReadAction.k_EUGCRead_ContinueReadingUntilFinished);
                        var jsonString = System.Text.Encoding.UTF8.GetString(buffer);
                        callback.Invoke(JsonUtility.FromJson<T>(jsonString), false);
                    }
                }
                else
                {
                    CashedUgcFileName = string.Empty;
                    EvtUgcDownloaded.Invoke(null);
                    callback?.Invoke(default, true);
                }
            });
        }

        /// <summary>
        /// Begins downloading the UGC file attached to this entry.
        /// Invokes <see cref="EvtUgcDownloaded"/> when complete.
        /// </summary>
        /// <param name="priority">Download priority hint passed to Steam.</param>
        /// <returns>True if a download was started; false if the handle is invalid.</returns>
        public bool StartUgcDownload(uint priority = 0)
        {
            if (UgcHandle == UGCHandle_t.Invalid)
                return false;

            var call = SteamRemoteStorage.UGCDownload(UgcHandle, priority);
            var callResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create();
            callResult.Set(call, HandleUgcDownloadResult);
            return true;
        }

        /// <summary>
        /// Begins downloading the UGC file attached to this entry.
        /// Invokes both <see cref="EvtUgcDownloaded"/> and <paramref name="callback"/> when complete.
        /// </summary>
        /// <param name="priority">Download priority hint.</param>
        /// <param name="callback">Receives the raw result and an IO-failure flag.</param>
        /// <returns>True if a download was started; false if the handle is invalid.</returns>
        public bool StartUgcDownload(uint priority, Action<RemoteStorageDownloadUGCResult_t, bool> callback)
        {
            if (UgcHandle == UGCHandle_t.Invalid)
                return false;

            var call = SteamRemoteStorage.UGCDownload(UgcHandle, priority);
            var callResult = CallResult<RemoteStorageDownloadUGCResult_t>.Create();
            callResult.Set(call, (p, e) =>
            {
                HandleUgcDownloadResult(p, e);
                callback?.Invoke(p, e);
            });
            return true;
        }

        /// <summary>Returns the download progress as a 0–1 fraction.</summary>
        public float UgcDownloadProgress()
        {
            SteamRemoteStorage.GetUGCDownloadProgress(UgcHandle, out int downloaded, out int expected);
            return downloaded / (float)expected;
        }

        private void HandleUgcDownloadResult(RemoteStorageDownloadUGCResult_t param, bool bIOFailure)
        {
            if (!bIOFailure && param.m_eResult == EResult.k_EResultOK)
            {
                CashedUgcFileName = param.m_pchFileName;
                EvtUgcDownloaded.Invoke(param.m_pchFileName);
            }
            else
            {
                CashedUgcFileName = string.Empty;
                EvtUgcDownloaded.Invoke(null);
            }
        }
    }
}
#endif
