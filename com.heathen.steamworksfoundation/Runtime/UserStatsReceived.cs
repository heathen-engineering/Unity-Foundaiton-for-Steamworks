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
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>Wraps the native <see cref="UserStatsReceived_t"/> callback result.</summary>
    [Serializable]
    public struct UserStatsReceived
    {
        public UserStatsReceived_t Data;
        public GameData Id     => Data.m_nGameID;
        public EResult  Result => Data.m_eResult;
        public UserData User   => Data.m_steamIDUser;

        public static implicit operator UserStatsReceived(UserStatsReceived_t native)   => new() { Data = native };
        public static implicit operator UserStatsReceived_t(UserStatsReceived heathen) => heathen.Data;
    }
}
#endif
