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
    /// <summary>
    /// Represents a Steam stat component.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Steamworks/Stat")]
    [HelpURL("https://heathen.group/kb/stats/")]
    public class SteamStatData : MonoBehaviour
    {
        /// <summary>
        /// The API name of the stat as defined in the Steamworks portal.
        /// </summary>
        public string apiName;

        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private List<string> mDelegates;

        /// <summary>
        /// Gets or sets the stat data using its API name.
        /// </summary>
        public StatData Data
        {
            get => apiName;
            set => apiName = value.ApiName;
        }

        /// <summary>
        /// Gets the integer value of the stat for the local user.
        /// </summary>
        /// <returns>The stat value.</returns>
        public int  IntValue()            => Data.IntValue();
        /// <summary>
        /// Gets the floating-point value of the stat for the local user.
        /// </summary>
        /// <returns>The stat value.</returns>
        public float FloatValue()         => Data.FloatValue();
        /// <summary>
        /// Sets the integer value of the stat for the local user.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void SetInt(int value)     => Data.Set(value);
        /// <summary>
        /// Sets the floating-point value of the stat for the local user.
        /// </summary>
        /// <param name="value">The value to set.</param>
        public void SetFloat(float value) => Data.Set(value);
        /// <summary>
        /// Requests the Steam client to store current stats.
        /// </summary>
        public void Store()               => Data.Store();

    }
}
#endif
