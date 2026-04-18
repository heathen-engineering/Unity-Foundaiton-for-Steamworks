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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Steamworks/Achievement")]
    [HelpURL("https://kb.heathen.group/steamworks/features/achievements")]
    public class SteamAchievementData : MonoBehaviour
    {
        /// <summary>The API name of the achievement as defined in the Steamworks portal.</summary>
        public string apiName;

        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private List<string> mDelegates;

        /// <summary>Gets or sets the achievement using its API name.</summary>
        public AchievementData Data
        {
            get => apiName;
            set => apiName = value.ApiName;
        }

        public void Unlock()      => Data.Unlock();
        public void Clear()       => Data.Clear();
        public void Store()       => Data.Store();

        public void SetAchieved(bool value)
        {
            if (value) Data.Unlock();
            else       Data.Clear();
        }

    }
}
#endif
