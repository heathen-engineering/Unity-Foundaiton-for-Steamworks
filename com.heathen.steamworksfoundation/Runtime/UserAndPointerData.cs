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
using System;
using UnityEngine.EventSystems;

namespace Heathen.SteamworksIntegration.UI
{
    /// <summary>
    /// Data structure containing both Steam user data and Unity pointer event data.
    /// </summary>
    [Serializable]
    public class UserAndPointerData
    {
        /// <summary>The user associated with the event.</summary>
        public UserData user;
        /// <summary>The pointer event data from Unity's event system.</summary>
        public PointerEventData PointerEventData;

        /// <summary>
        /// Initialises a new instance of the class.
        /// </summary>
        /// <param name="userData">The user data.</param>
        /// <param name="data">The pointer event data.</param>
        public UserAndPointerData(UserData userData, PointerEventData data)
        {
            user = userData;
            PointerEventData = data;
        }
    }
}
#endif
