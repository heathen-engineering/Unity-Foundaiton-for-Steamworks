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
using Heathen.SteamworksIntegration.UI;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Heathen.SteamworksIntegration
{
    [ModularEvents(typeof(SteamUserData))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserDataEvents : MonoBehaviour, IPointerClickHandler
    {
        [EventField]
        public UnityEvent<UserData, EPersonaChange> onChange;
        /// <summary>
        /// A <see cref="UnityEngine.Events.UnityEvent"/> that will be invoked when the user clicks the UI element
        /// </summary>
        [EventField]
        public UnityUserAndPointerDataEvent onClick;

        private SteamUserData _mInspector;

        private void Awake()
        {
            _mInspector = GetComponent<SteamUserData>();
            _mInspector.onChanged?.AddListener(onChange.Invoke);
        }

        private void OnDestroy()
        {
            if (_mInspector != null)
                _mInspector.onChanged?.RemoveListener(onChange.Invoke);
        }

        /// <summary>
        /// An implementation of <see cref="IPointerClickHandler"/> this will be invoked when the user clicks on the UI element
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            onClick.Invoke(new UserAndPointerData(_mInspector.Data, eventData));
        }
    }
}
#endif
