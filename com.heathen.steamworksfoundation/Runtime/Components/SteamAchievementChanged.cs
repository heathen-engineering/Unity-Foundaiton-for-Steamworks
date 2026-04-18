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
    /// <summary>Fires a UnityEvent when the tracked achievement's unlock state changes.</summary>
    [ModularEvents(typeof(SteamAchievementData))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAchievementData))]
    public class SteamAchievementChanged : MonoBehaviour
    {
        [EventField]
        public UnityEvent<bool> onChanged;

        private SteamAchievementData _mData;

        private void Awake()
        {
            _mData = GetComponent<SteamAchievementData>();
            API.StatsAndAchievements.Client.OnAchievementStatusChanged.AddListener(HandleStatusChanged);
        }

        private void OnDestroy()
        {
            API.StatsAndAchievements.Client.OnAchievementStatusChanged.RemoveListener(HandleStatusChanged);
        }

        private void HandleStatusChanged(string achievementName, bool achieved)
        {
            if (achievementName == _mData.apiName)
                onChanged?.Invoke(achieved);
        }

    }
}
#endif
