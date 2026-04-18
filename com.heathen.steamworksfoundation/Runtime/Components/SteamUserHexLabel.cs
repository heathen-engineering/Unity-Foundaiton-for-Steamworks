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
    [ModularComponent(typeof(SteamUserData), "Hex Labels", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserHexLabel : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamUserData _mSteamUserData;

        private void Awake()
        {
            _mSteamUserData = GetComponent<SteamUserData>();
            if (_mSteamUserData.Data.IsValid && label != null)
                label.text = _mSteamUserData.Data.HexId;
            _mSteamUserData.onChanged.AddListener(HandlePersonaStateChanged);
        }

        private void OnDestroy()
        {
            if (_mSteamUserData != null)
                _mSteamUserData.onChanged.RemoveListener(HandlePersonaStateChanged);
        }

        private void HandlePersonaStateChanged(UserData user, EPersonaChange flag)
        {
            if (label == null) return;
            label.text = _mSteamUserData.Data.IsValid ? _mSteamUserData.Data.HexId : string.Empty;
        }
    }
}
#endif
