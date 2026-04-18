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
    /// <summary>Displays the local user's rank on the tracked leaderboard in a TMP label.</summary>
    [ModularComponent(typeof(SteamLeaderboardData), "Ranks", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    [RequireComponent(typeof(SteamLeaderboardDataEvents))]
    public class SteamLeaderboardRank : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamLeaderboardData       _mInspector;
        private SteamLeaderboardDataEvents _mEvents;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLeaderboardData>();
            _mEvents    = GetComponent<SteamLeaderboardDataEvents>();

            if (_mEvents != null)
            {
                _mEvents.onChange.AddListener(HandleOnChanged);
                _mEvents.onRankChanged.AddListener(HandleRankChanged);
            }

            if (_mInspector.Data.IsValid)
                FetchCurrentRank();
        }

        private void OnDestroy()
        {
            if (_mEvents != null)
            {
                _mEvents.onChange.RemoveListener(HandleOnChanged);
                _mEvents.onRankChanged.RemoveListener(HandleRankChanged);
            }
        }

        private void FetchCurrentRank()
        {
            _mInspector.Data.GetUserEntry(0, (entry, ioError) =>
            {
                if (label == null) return;
                label.text = (!ioError && entry != null) ? entry.Rank.ToString() : string.Empty;
            });
        }

        private void HandleOnChanged()
        {
            if (_mInspector.Data.IsValid)
                FetchCurrentRank();
            else if (label != null)
                label.text = string.Empty;
        }

        private void HandleRankChanged(LeaderboardScoreUploaded result)
        {
            if (label != null)
                label.text = result.GlobalRankNew.ToString();
        }

    }
}
#endif
