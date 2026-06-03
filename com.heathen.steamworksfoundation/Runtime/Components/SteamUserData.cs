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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam user component.
    /// </summary>
    [AddComponentMenu("Steamworks/User")]
    [DisallowMultipleComponent]
    public class SteamUserData : MonoBehaviour, ISteamUserData
    {
        /// <summary>
        /// Should this component automatically target the local user?
        /// </summary>
        public bool localUser = false;

        /// <summary>
        /// Gets or sets the user data.
        /// </summary>
        public UserData Data
        {
            get => _mData;
            set
            {
                _mData = value;
                onChanged?.Invoke(value, (EPersonaChange)int.MaxValue);
            }
        }

        /// <summary>
        /// Event invoked when the user's data or persona state changes.
        /// </summary>
        [HideInInspector]
        public UnityEvent<UserData, EPersonaChange> onChanged;

        [FormerlySerializedAs("m_Delegates")] [SerializeField]
        private List<string> mDelegates;

        private UserData _mData;

        private void Awake()
        {
            SteamTools.Events.OnPersonaStateChange += GlobalPersonaUpdate;
        }

        private void Start()
        {
            SteamTools.Interface.WhenReady(HandleInitialization);
        }

        private void OnDestroy()
        {
            SteamTools.Events.OnPersonaStateChange -= GlobalPersonaUpdate;
        }

        private void HandleInitialization()
        {
            if (localUser) Data = UserData.Me;
        }

        private void GlobalPersonaUpdate(UserData user, EPersonaChange changeFlag)
        {
            if (user == Data)
                onChanged?.Invoke(user, changeFlag);
        }

    }
}
#endif
