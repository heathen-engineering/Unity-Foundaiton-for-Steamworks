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
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>Displays the Steam persona name of the tracked user in a TMP label.</summary>
    [ModularComponent(typeof(SteamUserData), "Names", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserName : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamUserData _mUserData;

        private void Awake()
        {
            _mUserData = GetComponent<SteamUserData>();
            if (_mUserData.Data.IsValid && label != null)
                label.text = _mUserData.Data.Name;
            _mUserData.onChanged.AddListener(HandlePersonaStateChange);
        }

        private void OnDestroy()
        {
            if (_mUserData != null)
                _mUserData.onChanged.RemoveListener(HandlePersonaStateChange);
        }

        private void HandlePersonaStateChange(UserData user, EPersonaChange flag)
        {
            if (label != null && _mUserData.Data.IsValid)
                label.text = _mUserData.Data.Name;
        }

    }
}
#endif
