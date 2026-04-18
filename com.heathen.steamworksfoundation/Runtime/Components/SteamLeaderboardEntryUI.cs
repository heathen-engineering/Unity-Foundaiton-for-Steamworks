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
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// A ready-made implementation of <see cref="ILeaderboardEntryDisplay"/> for use as
    /// the <c>entryTemplate</c> prefab on <see cref="SteamLeaderboardDisplay"/>.
    /// </summary>
    [RequireComponent(typeof(SteamUserData))]
    public class SteamLeaderboardEntryUI : MonoBehaviour, ILeaderboardEntryDisplay
    {
        [SerializeField] private TMPro.TextMeshProUGUI score;
        [SerializeField] private TMPro.TextMeshProUGUI rank;

        private SteamUserData  _userData;
        private LeaderboardEntry _entry;

        public LeaderboardEntry Entry
        {
            get => _entry;
            set => SetEntry(value);
        }

        private void Awake()
        {
            _userData = GetComponent<SteamUserData>();
        }

        private void SetEntry(LeaderboardEntry entry)
        {
            _entry = entry;
            if (_userData != null)
                _userData.Data = entry.User;
            if (score != null) score.text = entry.Score.ToString();
            if (rank  != null) rank.text  = entry.Rank.ToString();
        }

    }
}
#endif
