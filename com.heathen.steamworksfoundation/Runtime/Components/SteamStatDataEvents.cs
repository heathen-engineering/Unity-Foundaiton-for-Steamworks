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
    /// Modular events component for <see cref="SteamStatData"/>.
    /// Wire up <see cref="onIntChanged"/> for integer stats or <see cref="onFloatChanged"/> for float stats.
    /// Both events fire on every stat refresh — wire only the one that matches the stat type.
    /// </summary>
    [ModularEvents(typeof(SteamStatData))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamStatData))]
    public class SteamStatDataEvents : MonoBehaviour
    {
        [EventField]
        public UnityEvent<int> onIntChanged;
        [EventField]
        public UnityEvent<float> onFloatChanged;

        private SteamStatData _mData;
        private UnityAction<UserStatsReceived> _onStatsReceivedDelegate;

        private void Awake()
        {
            _mData = GetComponent<SteamStatData>();
            _onStatsReceivedDelegate = HandleStatsReceived;
            API.StatsAndAchievements.Client.OnUserStatsReceived.AddListener(_onStatsReceivedDelegate);
        }

        private void OnDestroy()
        {
            API.StatsAndAchievements.Client.OnUserStatsReceived.RemoveListener(_onStatsReceivedDelegate);
        }

        private void HandleStatsReceived(UserStatsReceived _)
        {
            onIntChanged?.Invoke(_mData.IntValue());
            onFloatChanged?.Invoke(_mData.FloatValue());
        }
    }
}
#endif
