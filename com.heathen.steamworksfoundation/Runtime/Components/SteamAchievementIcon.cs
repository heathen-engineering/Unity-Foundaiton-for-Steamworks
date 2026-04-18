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
using UnityEngine.UI;

namespace Heathen.SteamworksIntegration
{
    /// <summary>Displays the icon for a Steam achievement in a RawImage, updating when the status changes.</summary>
    [ModularComponent(typeof(SteamAchievementData), "Icons", nameof(image))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAchievementData))]
    public class SteamAchievementIcon : MonoBehaviour
    {
        public RawImage image;

        private SteamAchievementData _mData;

        private void Awake()
        {
            _mData = GetComponent<SteamAchievementData>();
        }

        private void Start()
        {
            API.StatsAndAchievements.Client.OnAchievementStatusChanged.AddListener(HandleStatusChanged);
            SteamTools.Interface.WhenReady(Refresh);
        }

        private void OnDestroy()
        {
            API.StatsAndAchievements.Client.OnAchievementStatusChanged.RemoveListener(HandleStatusChanged);
        }

        private void HandleStatusChanged(string achievementName, bool achieved)
        {
            if (achievementName == _mData.apiName)
                Refresh();
        }

        public void Refresh()
        {
            if (image != null && !string.IsNullOrEmpty(_mData.apiName))
            {
                API.StatsAndAchievements.Client.GetAchievementIcon(_mData.apiName, texture =>
                {
                    if (texture && image)
                        image.texture = texture;
                });
            }
        }

    }
}
#endif
