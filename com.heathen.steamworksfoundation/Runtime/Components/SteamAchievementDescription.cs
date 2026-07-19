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
    /// Displays the localised description of a Steam achievement.
    /// </summary>
    [ModularComponent(typeof(SteamAchievementData), "Descriptions", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamAchievementData))]
    [HelpURL("https://heathen.group/kb/steam-features-achievements/")]
    public class SteamAchievementDescription : MonoBehaviour
    {
        /// <summary>
        /// The TextMeshPro label to display the achievement description in.
        /// </summary>
        public TMPro.TextMeshProUGUI label;

        private SteamAchievementData _mData;

        private void Awake()
        {
            _mData = GetComponent<SteamAchievementData>();
        }

        private void Start()
        {
            SteamTools.Interface.WhenReady(Refresh);
        }

        /// <summary>
        /// Refreshes the label with the localised achievement description.
        /// </summary>
        public void Refresh()
        {
            if (label != null && !string.IsNullOrEmpty(_mData.apiName))
                label.text = API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(
                    _mData.apiName, AchievementAttributes.Desc);
        }

    }
}
#endif
