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
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Modular events component for <see cref="SteamLeaderboardData"/>.
    /// </summary>
    [ModularEvents(typeof(SteamLeaderboardData))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamLeaderboardData))]
    public class SteamLeaderboardDataEvents : MonoBehaviour, ISteamLeaderboardDataEvents
    {
        /// <summary>Event invoked when the leaderboard data changes.</summary>
        [EventField] public UnityEvent                           onChange;
        /// <summary>Event invoked when a leaderboard is found or created.</summary>
        [EventField] public UnityEvent                           onFindOrCreate;
        /// <summary>Event invoked when a leaderboard find or create operation fails.</summary>
        [EventField] public UnityEvent                           onFindOrCreateFailure;
        /// <summary>Event invoked when a score is successfully uploaded to the leaderboard.</summary>
        [EventField] public UnityEvent<LeaderboardScoreUploaded> onScoreUploaded;
        /// <summary>Event invoked when the local user's rank on the leaderboard changes.</summary>
        [EventField] public UnityEvent<LeaderboardScoreUploaded> onRankChanged;

        UnityEvent ISteamLeaderboardDataEvents.onChange => onChange;
        UnityEvent ISteamLeaderboardDataEvents.onFindOrCreate => onFindOrCreate;
        UnityEvent ISteamLeaderboardDataEvents.onFindOrCreateFailure => onFindOrCreateFailure;
        UnityEvent<LeaderboardScoreUploaded> ISteamLeaderboardDataEvents.onRankChanged => onRankChanged;

        private SteamLeaderboardData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamLeaderboardData>();
            API.Leaderboards.Client.OnScoreUploaded.AddListener(HandleScoreUploaded);
        }

        private void OnDestroy()
        {
            API.Leaderboards.Client.OnScoreUploaded.RemoveListener(HandleScoreUploaded);
        }

        private void HandleScoreUploaded(LeaderboardScoreUploaded result, bool ioError)
        {
            if (!ioError && result.Leaderboard == _mInspector.Data)
            {
                onScoreUploaded?.Invoke(result);
                if (result.GlobalRankNew != result.GlobalRankPrevious)
                    onRankChanged?.Invoke(result);
            }
        }

    }
}
#endif
