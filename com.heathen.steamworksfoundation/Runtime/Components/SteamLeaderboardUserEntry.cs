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
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Fetches and displays the local user's entry on the tracked leaderboard
    /// via an <see cref="ILeaderboardEntryDisplay"/> component reference.
    /// </summary>
    [ModularComponent(typeof(SteamLeaderboardData), "User Entries", nameof(entryUI))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    public class SteamLeaderboardUserEntry : MonoBehaviour
    {
        [Tooltip("A component that implements ILeaderboardEntryDisplay, e.g. SteamLeaderboardEntryUI.")]
        public SteamLeaderboardEntryUI entryUI;

        private SteamLeaderboardData        _mInspector;
        private ISteamLeaderboardDataEvents _mEvents;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLeaderboardData>();
            _mEvents    = GetComponent<ISteamLeaderboardDataEvents>();

            if (_mEvents != null)
            {
                _mEvents.onChange.AddListener(Refresh);
                _mEvents.onRankChanged.AddListener(HandleRankChanged);
            }
        }

        private void OnDestroy()
        {
            if (_mEvents != null)
            {
                _mEvents.onChange.RemoveListener(Refresh);
                _mEvents.onRankChanged.RemoveListener(HandleRankChanged);
            }
        }

        public void Refresh()
        {
            if (!_mInspector.Data.IsValid || entryUI == null) return;

            _mInspector.Data.GetUserEntry(0, (entry, ioError) =>
            {
                if (entryUI == null) return;

                if (!ioError && entry != null)
                {
                    entryUI.Entry = entry;
                }
                else
                {
                    entryUI.Entry = new LeaderboardEntry
                    {
                        Entry   = new LeaderboardEntry_t { m_steamIDUser = UserData.Me, m_nGlobalRank = 0, m_nScore = 0 },
                        Details = new int[0],
                    };
                }
            });
        }

        private void HandleRankChanged(LeaderboardScoreUploaded result) => Refresh();

    }
}
#endif
