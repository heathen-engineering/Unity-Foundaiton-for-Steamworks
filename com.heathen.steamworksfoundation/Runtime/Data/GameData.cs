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
using Steamworks;
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Game as defined by <see cref="CGameID"/> in the Steam API
    /// </summary>
    [Serializable]
    public struct GameData : IEquatable<AppId_t>, IEquatable<CGameID>, IEquatable<uint>, IEquatable<ulong>, IEquatable<AppData>, IComparable<AppData>, IComparable<AppId_t>, IComparable<uint>, IComparable<ulong>
    {
        [SerializeField]
        private ulong id;
        /// <summary>
        /// Returns the <see cref="GameData"/> for the current program
        /// </summary>
        public static GameData Me => API.App.Id.AppId;
        /// <summary>
        /// Returns the native <see cref="CGameID"/> representing the game
        /// </summary>
        public readonly CGameID GameId => new CGameID(id);
        /// <summary>
        /// Returns the primitive <see cref="ulong"/> representing the game
        /// </summary>
        public readonly ulong Id => GameId.m_GameID;
        /// <summary>
        /// Returns the <see cref="AppData"/> representing the game
        /// </summary>
        public readonly AppData App => GameId.AppID();
        /// <summary>
        /// Returns the Mod flag from the <see cref="CGameID"/> structure for this game
        /// </summary>
        public readonly bool IsMod => GameId.IsMod();
        /// <summary>
        /// Returns the P2P File flag from the <see cref="CGameID"/> structure for this game
        /// </summary>
        public readonly bool IsP2PFile => GameId.IsP2PFile();
        /// <summary>
        /// Returns the Shortcut flag from the <see cref="CGameID"/> structure for this game
        /// </summary>
        public readonly bool IsShortcut => GameId.IsShortcut();
        /// <summary>
        /// Returns the Steam App flag from the <see cref="CGameID"/> structure for this game
        /// </summary>
        public readonly bool IsSteamApp => GameId.IsSteamApp();
        /// <summary>
        /// True if this is a valid Game ID according to Steam API
        /// </summary>
        public readonly bool IsValid => GameId.IsValid();
        /// <summary>
        /// Returns the Mod ID from the <see cref="CGameID"/> structure for this game
        /// </summary>
        public readonly uint ModID => GameId.ModID();
        /// <summary>
        /// Returns true if this <see cref="GameData"/> represents the current program
        /// </summary>
        public readonly bool IsMe => this == Me;
        /// <summary>
        /// Returns the <see cref="CGameID.EGameIDType"/> for this game, this is similar to checking the <see cref="IsMod"/>, <see cref="IsShortcut"/>, <see cref="IsP2PFile"/> or <see cref="IsSteamApp"/> flags
        /// </summary>
        public readonly CGameID.EGameIDType Type => GameId.Type();
        /// <summary>
        /// Returns the <see cref="GameData"/> that represents this program
        /// </summary>
        /// <returns>The <see cref="GameData"/> that represents this program</returns>
        public static GameData Get() => Me;
        /// <summary>
        /// Returns the <see cref="GameData"/> given a native <see cref="CGameID"/>
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns>The <see cref="GameData"/> represented by the provided <see cref="CGameID"/></returns>
        public static GameData Get(CGameID gameId) => gameId;
        /// <summary>
        /// Returns the <see cref="GameData"/> given a primitive <see cref="ulong"/> representing its Game ID
        /// </summary>
        /// <param name="gameId">The <see cref="GameData"/> represented by the provided game id</param>
        /// <returns></returns>
        public static GameData Get(ulong gameId) => gameId;
        /// <summary>
        /// Returns the <see cref="GameData"/> given a primitive <see cref="uint"/> representing its App ID
        /// </summary>
        /// <param name="appId">The <see cref="GameData"/> represented by the provided app id</param>
        /// <returns></returns>
        public static GameData Get(uint appId) => appId;
        /// <summary>
        /// Returns the <see cref="GameData"/> given a native <see cref="AppId_t"/> representing its App ID
        /// </summary>
        /// <param name="appId">The <see cref="GameData"/> represented by the provided app id</param>
        /// <returns></returns>
        public static GameData Get(AppId_t appId) => appId;
        #region Boilerplate
        public readonly int CompareTo(AppData other)
        {
            return App.CompareTo(other.AppId);
        }

        public readonly int CompareTo(GameData other)
        {
            return Id.CompareTo(other.Id);
        }

        public readonly int CompareTo(AppId_t other)
        {
            return App.CompareTo(other);
        }

        public readonly int CompareTo(ulong other)
        {
            return GameId.m_GameID.CompareTo(other);
        }

        public readonly int CompareTo(uint other)
        {
            return App.CompareTo(other);
        }

        public readonly override string ToString()
        {
            return GameId.ToString();
        }

        public readonly bool Equals(AppData other)
        {
            return App.Equals(other.AppId);
        }

        public readonly bool Equals(GameData other)
        {
            return Id.Equals(other.Id);
        }

        public readonly bool Equals(AppId_t other)
        {
            return App.Id.Equals(other.m_AppId);
        }

        public readonly bool Equals(uint other)
        {
            return App.Id.Equals(other);
        }

        public readonly bool Equals(CGameID other)
        {
            return GameId.AppID().Equals(other.AppID());
        }

        public readonly bool Equals(ulong other)
        {
            return GameId.Equals(new CGameID(other));
        }

        public readonly override bool Equals(object obj)
        {
            return GameId.m_GameID.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return GameId.GetHashCode();
        }

        public static bool operator ==(GameData l, GameData r) => l.GameId.m_GameID == r.GameId.m_GameID;
        public static bool operator ==(AppId_t l, GameData r) => l == r.App;
        public static bool operator ==(GameData l, AppId_t r) => l.App == r;
        public static bool operator !=(GameData l, GameData r) => l.GameId.m_GameID != r.GameId.m_GameID;
        public static bool operator !=(AppId_t l, GameData r) => l != r.App;
        public static bool operator !=(GameData l, AppId_t r) => l.App != r;

        public static implicit operator GameData(CGameID id) => new GameData { id = id.m_GameID };
        public static implicit operator uint(GameData c) => c.App.Id;
        public static implicit operator ulong(GameData c) => c.Id;
        public static implicit operator GameData(ulong id) => new GameData { id = id };
        public static implicit operator GameData(uint id) => new GameData { id = new CGameID(new AppId_t(id)).m_GameID };
        public static implicit operator AppId_t(GameData c) => c.App;
        public static implicit operator GameData(AppId_t id) => new CGameID(id);
        #endregion
    }
}
#endif
