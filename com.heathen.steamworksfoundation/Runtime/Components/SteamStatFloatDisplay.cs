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
    /// <summary>Displays a float Steam stat value in a TMP label.</summary>
    [ModularComponent(typeof(SteamStatData), "Float Display", nameof(label))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamStatData))]
    public class SteamStatFloatDisplay : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI label;
        [Tooltip("Optional format string, e.g. \"{0:F2} m\". Leave empty for the raw value.")]
        public string format;

        private SteamStatData _mData;

        private void Awake()
        {
            _mData = GetComponent<SteamStatData>();
        }

        private void Start()
        {
            SteamTools.Interface.WhenReady(Refresh);
        }

        public void Refresh()
        {
            if (label == null) return;
            float value = _mData.Data.FloatValue();
            label.text = string.IsNullOrEmpty(format) ? value.ToString() : string.Format(format, value);
        }

    }
}
#endif
