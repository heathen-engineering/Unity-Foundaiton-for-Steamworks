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
        public string CachedUgcFileName = string.Empty;
        public bool   HasCachedUgcFileName => !string.IsNullOrEmpty(CachedUgcFileName);

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

            API.Leaderboards.Client.DownloadEntryUgc(UgcHandle, 0, (result, bIOFailure) =>
            {
                if (!bIOFailure && result.m_eResult == EResult.k_EResultOK)
                {
                    CachedUgcFileName = result.m_pchFileName;
                    EvtUgcDownloaded.Invoke(result.m_pchFileName);

                    if (callback != null)
                    {
                        var buffer = new byte[result.m_nSizeInBytes];
                        SteamRemoteStorage.UGCRead(result.m_hFile, buffer, result.m_nSizeInBytes, 0, EUGCReadAction.k_EUGCRead_ContinueReadingUntilFinished);
                        callback.Invoke(JsonUtility.FromJson<T>(System.Text.Encoding.UTF8.GetString(buffer)), false);
                    }
                }
                else
                {
                    CachedUgcFileName = string.Empty;
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
        public void StartUgcDownload(uint priority = 0)
        {
            if (UgcHandle == UGCHandle_t.Invalid)
                return;

            API.Leaderboards.Client.DownloadEntryUgc(UgcHandle, priority, HandleUgcDownloadResult);
        }

        /// <summary>
        /// Begins downloading the UGC file attached to this entry.
        /// Invokes both <see cref="EvtUgcDownloaded"/> and <paramref name="callback"/> when complete.
        /// </summary>
        /// <param name="priority">Download priority hint.</param>
        /// <param name="callback">Receives the raw result and an IO-failure flag.</param>
        public void StartUgcDownload(uint priority, Action<RemoteStorageDownloadUGCResult_t, bool> callback)
        {
            if (UgcHandle == UGCHandle_t.Invalid)
                return;

            API.Leaderboards.Client.DownloadEntryUgc(UgcHandle, priority, (p, e) =>
            {
                HandleUgcDownloadResult(p, e);
                callback?.Invoke(p, e);
            });
        }

        /// <summary>Returns the download progress as a 0–1 fraction.</summary>
        public float UgcDownloadProgress()
        {
            SteamRemoteStorage.GetUGCDownloadProgress(UgcHandle, out int downloaded, out int expected);
            return expected == 0 ? 0f : downloaded / (float)expected;
        }

        private void HandleUgcDownloadResult(RemoteStorageDownloadUGCResult_t param, bool bIOFailure)
        {
            if (!bIOFailure && param.m_eResult == EResult.k_EResultOK)
            {
                CachedUgcFileName = param.m_pchFileName;
                EvtUgcDownloaded.Invoke(param.m_pchFileName);
            }
            else
            {
                CachedUgcFileName = string.Empty;
                EvtUgcDownloaded.Invoke(null);
            }
        }
    }
}
#endif
