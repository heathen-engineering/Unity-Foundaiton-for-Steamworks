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
    /// <summary>
    /// Represents information about the game a Steam friend is currently playing.
    /// </summary>
    [Serializable]
    public struct FriendGameInfo
    {
        /// <summary>Raw game session data from Steamworks.</summary>
        public FriendGameInfo_t Data;

        /// <summary>The game the friend is currently playing.</summary>
        public readonly GameData Game => Data.m_gameID;

        /// <summary>The game server IP address as a string.</summary>
        public readonly string IpAddress => API.Utilities.IPUintToString(Data.m_unGameIP);

        /// <summary>The game server IP address as a uint.</summary>
        public readonly uint IpInt => Data.m_unGameIP;

        /// <summary>The game server port.</summary>
        public readonly ushort GamePort => Data.m_usGamePort;

        /// <summary>The query port.</summary>
        public readonly ushort QueryPort => Data.m_usQueryPort;

        /// <summary>The Steam ID of the lobby the friend is in, if any.</summary>
        public readonly CSteamID LobbyId => Data.m_steamIDLobby;

        public static implicit operator FriendGameInfo(FriendGameInfo_t native) => new() { Data = native };
        public static implicit operator FriendGameInfo_t(FriendGameInfo heathen) => heathen.Data;
    }
}
#endif
