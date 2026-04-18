// Copyright 2024 Heathen Engineering Limited
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
#if !DISABLESTEAMWORKS  && STEAM_INSTALLED

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Used to indicate what user left a lobby and the state they left under.
    /// </summary>
    [System.Serializable]
    public struct UserLobbyLeaveData
    {
        /// <summary>
        /// The user that left the lobby.
        /// </summary>
        public UserData user;
        /// <summary>
        /// The state the user left under.
        /// </summary>
        public Steamworks.EChatMemberStateChange state;
    }
}
#endif
