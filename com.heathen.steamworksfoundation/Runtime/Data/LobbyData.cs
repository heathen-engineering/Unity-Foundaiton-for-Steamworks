// Copyright 2024 Heathen Engineering Limited
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#if !DISABLESTEAMWORKS && STEAM_INSTALLED
using Steamworks;
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the data related to a Steamworks lobby. Provides properties and methods to manage and interact with a Steam lobby, including its members, metadata, and configuration.
    /// A lobby is a communication and matchmaking structure in Steamworks, providing a way for players to form groups, exchange data, and prepare for multiplayer sessions.
    /// </summary>
    /// <remarks>
    /// This structure includes data such as the lobby's owner, type, members, metadata, and game server information. It also exposes functionality for checking lobby-related properties and managing specific lobby settings.
    /// Additionally, the struct implements equality checks with <c>CSteamID</c>, <c>ulong</c>, and other <c>LobbyData</c> instances for convenience.
    /// </remarks>
    [Serializable]
    public struct LobbyData : IEquatable<CSteamID>, IEquatable<ulong>, IEquatable<LobbyData>
    {
        /// <summary>
        /// Represents the unique identifier for a Steam lobby. This value is backed by an unsigned 64-bit integer
        /// and is used to uniquely distinguish the lobby in Steamworks services.
        /// </summary>
        private ulong _id;

        /// <summary>
        /// Provides access to the unique Steam identifier of the lobby, represented as a <see cref="CSteamID"/> value.
        /// This identifier is used to interact with Steamworks services associated with the lobby.
        /// </summary>
        public readonly CSteamID SteamId => new(_id);

        /// <summary>
        /// Represents the unique account identifier associated with a Steam user involved in the lobby.
        /// This value is derived from the SteamID and is encapsulated within the <see cref="AccountID_t"/> structure.
        /// </summary>
        public readonly AccountID_t AccountId => SteamId.GetAccountID();

        /// <summary>
        /// Represents the unique identifier for a friend account associated with the lobby. This is derived from the
        /// Steam account's numeric AccountID and is represented as an unsigned 32-bit integer.
        /// </summary>
        public readonly uint FriendId => AccountId.m_AccountID;

        /// <summary>
        /// Represents the hexadecimal string representation of the Friend ID associated with this lobby.
        /// The value is derived by converting the underlying Friend ID to its hexadecimal format.
        /// </summary>
        public readonly string HexId => FriendId.ToString("X");

        /// <summary>
        /// Indicates whether the current lobby data represents a valid Steam lobby.
        /// This value is determined by checking if the underlying Steam ID is not
        /// null, if it corresponds to a chat account type, and if it exists within
        /// the public Steam universe. Returns <c>true</c> if all conditions are
        /// satisfied; otherwise, returns <c>false</c>.
        /// </summary>
        public readonly bool IsValid
        {
            get
            {
                var sId = SteamId;
                if (sId == CSteamID.Nil
                    || sId.GetEAccountType() != EAccountType.k_EAccountTypeChat
                    || sId.GetEUniverse() != EUniverse.k_EUniversePublic)
                    return false;
                else
                    return true;
            }
        }

        #region Constants

        /// <summary>
        /// Represents the key used to store or retrieve a lobby's name in its metadata.
        /// This field is commonly used for identifying or displaying the name associated with the lobby.
        /// </summary>
        public const string DataName = "name";

        /// <summary>
        /// Represents a standard metadata key used to store the version of the game in both lobby and member metadata.
        /// This value is commonly used to track or verify the game version across connected lobby members.
        /// </summary>
        public const string DataVersion = "z_heathenGameVersion";

        /// <summary>
        /// Represents a standard metadata field used to indicate that a user is ready to play.
        /// This field is primarily used in the context of lobby member metadata within the Steamworks integration.
        /// </summary>
        public const string DataReady = "z_heathenReady";

        /// <summary>
        /// Represents a metadata field used to track Steam users that should leave the lobby.
        /// This field contains a string formatted as a list of CSteamIDs, where each ID is enclosed in brackets,
        /// e.g., [123456789][987654321]. When present, this indicates that the specified users should not join
        /// this lobby or must leave if they are already members.
        /// </summary>
        public const string DataKick = "z_heathenKick";

        /// <summary>
        /// Represents the Heathen standard metadata field used to indicate the mode of the lobby,
        /// such as party, session, or general. If this value is not set, the mode is assumed to be general.
        /// </summary>
        public const string DataMode = "z_heathenMode";

        /// <summary>
        /// Represents the metadata field used to define the type of lobby in the Heathen Steamworks Integration.
        /// This value determines the lobby's visibility and accessibility settings such as private, friends-only, public, or invisible.
        /// </summary>
        public const string DataType = "z_heathenType";

        /// <summary>
        /// A constant string representing the metadata key used to identify session lobbies in Heathen's Steamworks integration.
        /// This key is primarily used in the context of party lobbies to distinguish lobbies that represent active game sessions.
        /// </summary>
        public const string DataSessionLobby = "z_heathenSessionLobby";

        /// <summary>
        /// Represents the lobby mode set to "General." This constant is used as a marker to categorise a lobby
        /// where no specific session or party constraints are applied, allowing for a general-purpose lobby setup.
        /// </summary>
        public const string DataModeGeneral = "General";

        /// <summary>
        /// Represents a constant string used to denote that the lobby operates in "Session" mode.
        /// This mode is typically associated with sessions where the lobby functions as a temporary environment
        /// for collaborative or multiplayer interactions.
        /// </summary>
        public const string DataModeSession = "Session";

        /// <summary>
        /// Represents the data mode value for a Steam lobby that designates the lobby as a "Party."
        /// This constant is used internally to identify and manage lobbies operating in the Party context.
        /// </summary>
        public const string DataModeParty = "Party";

        #endregion

        #region Factory Get Methods

        /// <summary>
        /// Retrieves a lobby based on a provided account ID represented as a hexadecimal string.
        /// </summary>
        /// <param name="accountId">The hexadecimal string representing the account ID of the lobby.</param>
        /// <returns>The corresponding LobbyData object if the account ID is valid; otherwise, a default invalid LobbyData instance.</returns>
        public static LobbyData Get(string accountId)
        {
            try
            {
                var id = Convert.ToUInt32(accountId, 16);
                if (id > 0)
                    return Get(id);
                else
                    return CSteamID.Nil;
            }
            catch(Exception ex)
            {
                Debug.LogWarning("Failed to parse account Id: " + ex.Message);
                return CSteamID.Nil;
            }
        }

        /// <summary>
        /// Retrieves the lobby data associated with the specified account ID.
        /// </summary>
        /// <param name="accountId">The account ID of the desired lobby.</param>
        /// <returns>An instance of LobbyData representing the lobby identified by the given account ID.</returns>
        public static LobbyData Get(uint accountId) => new CSteamID(new AccountID_t(accountId), 393216,
            EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat);

        /// <summary>
        /// Retrieves the lobby associated with the specified account ID.
        /// </summary>
        /// <param name="accountId">The account ID of the lobby to retrieve.</param>
        /// <returns>A LobbyData object representing the specified lobby.</returns>
        public static LobbyData Get(AccountID_t accountId) => new CSteamID(accountId, 393216,
            EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeChat);

        /// <summary>
        /// Retrieves a LobbyData instance corresponding to the specified lobby identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the lobby.</param>
        /// <returns>A LobbyData structure representing the specified lobby.</returns>
        public static LobbyData Get(ulong id) => new LobbyData { _id = id };

        /// <summary>
        /// Retrieves a <see cref="LobbyData"/> object for a given CSteamID.
        /// </summary>
        /// <param name="id">The CSteamID representing the Steam lobby.</param>
        /// <returns>A <see cref="LobbyData"/> object that represents the specified lobby.</returns>
        public static LobbyData Get(CSteamID id) => new LobbyData { _id = id.m_SteamID };

        #endregion

        #region Boilerplate

        public int CompareTo(CSteamID other)
        {
            return _id.CompareTo(other);
        }

        public int CompareTo(ulong other)
        {
            return _id.CompareTo(other);
        }

        public override string ToString()
        {
            return HexId;
        }

        public bool Equals(CSteamID other)
        {
            return _id.Equals(other.m_SteamID);
        }

        public bool Equals(ulong other)
        {
            return _id.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return _id.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public bool Equals(LobbyData other)
        {
            return _id.Equals(other._id);
        }

        public static bool operator ==(LobbyData l, LobbyData r) => l._id == r._id;
        public static bool operator ==(CSteamID l, LobbyData r) => l.m_SteamID == r._id;
        public static bool operator ==(LobbyData l, CSteamID r) => l._id == r.m_SteamID;
        public static bool operator ==(LobbyData l, ulong r) => l._id == r;
        public static bool operator ==(ulong l, LobbyData r) => l == r._id;
        public static bool operator !=(LobbyData l, LobbyData r) => l._id != r._id;
        public static bool operator !=(CSteamID l, LobbyData r) => l.m_SteamID != r._id;
        public static bool operator !=(LobbyData l, CSteamID r) => l._id != r.m_SteamID;
        public static bool operator !=(LobbyData l, ulong r) => l._id != r;
        public static bool operator !=(ulong l, LobbyData r) => l != r._id;

        public static implicit operator CSteamID(LobbyData c) => c.SteamId;
        public static implicit operator LobbyData(CSteamID id) => new LobbyData { _id = id.m_SteamID };
        public static implicit operator ulong(LobbyData id) => id._id;
        public static implicit operator LobbyData(ulong id) => new LobbyData { _id = id };
        public static implicit operator LobbyData(AccountID_t id) => Get(id);
        public static implicit operator LobbyData(uint id) => Get(id);
        public static implicit operator LobbyData(string id) => Get(id);

        #endregion
    }
}
#endif
