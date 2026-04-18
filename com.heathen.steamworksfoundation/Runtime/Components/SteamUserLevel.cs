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
using System.Collections;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>Displays the Steam level of the tracked user in a TMP label.</summary>
    [ModularComponent(typeof(SteamUserData), "Levels", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserLevel : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;

        private SteamUserData _mUserData;
        private bool _levelRetryScheduled;

        private void Awake()
        {
            _mUserData = GetComponent<SteamUserData>();
            _mUserData.onChanged.AddListener(HandlePersonaStateChange);
            UpdateLevel();
        }

        private void OnDestroy()
        {
            if (_mUserData != null)
                _mUserData.onChanged.RemoveListener(HandlePersonaStateChange);
        }

        private void UpdateLevel()
        {
            if (label == null || !_mUserData.Data.IsValid) return;
            var level = _mUserData.Data.Level;
            label.text = level.ToString();
            if (level == 0 && !_levelRetryScheduled)
            {
                _levelRetryScheduled = true;
                StartCoroutine(RetryLevelNextFrame());
            }
        }

        private IEnumerator RetryLevelNextFrame()
        {
            yield return null;
            _levelRetryScheduled = false;
            if (label != null && _mUserData != null && _mUserData.Data.IsValid)
                label.text = _mUserData.Data.Level.ToString();
        }

        private void HandlePersonaStateChange(UserData user, EPersonaChange flag)
        {
            UpdateLevel();
        }

    }
}
#endif
