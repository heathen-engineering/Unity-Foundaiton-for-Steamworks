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
    /// Represents a Steam Clan (Group) and provides its identity data.
    /// </summary>
    [Serializable]
    public struct ClanData : IEquatable<CSteamID>, IEquatable<ClanData>, IEquatable<ulong>, IComparable<CSteamID>,
        IComparable<ClanData>, IComparable<ulong>
    {
        /// <summary>
        /// The unique 64-bit identifier representing the Steam clan.
        /// </summary>
        [SerializeField] private ulong id;

        /// <summary>
        /// Represents the Steam identifier (CSteamID) associated with this clan.
        /// </summary>
        public readonly CSteamID SteamId => new CSteamID(id);

        /// <summary>
        /// Represents the account identifier associated with the clan's Steam data.
        /// </summary>
        public readonly AccountID_t AccountId => SteamId.GetAccountID();

        /// <summary>
        /// The identifier representing the account ID of the friend associated with this clan.
        /// </summary>
        public readonly uint FriendId => SteamId.GetAccountID().m_AccountID;

        /// <summary>
        /// Indicates whether the current clan data instance is valid.
        /// </summary>
        public readonly bool IsValid
        {
            get
            {
                var sID = SteamId;
                if (sID == CSteamID.Nil
                    || sID.GetEAccountType() != EAccountType.k_EAccountTypeClan
                    || sID.GetEUniverse() != EUniverse.k_EUniversePublic)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Retrieves the clan data associated with the specified account ID.
        /// </summary>
        public static ClanData Get(uint accountId) => new CSteamID(new AccountID_t(accountId),
            EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeClan);

        /// <summary>
        /// Retrieves the clan associated with the specified account ID.
        /// </summary>
        public static ClanData Get(AccountID_t accountId) =>
            new CSteamID(accountId, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeClan);

        /// <summary>
        /// Retrieves a ClanData for the specified id.
        /// </summary>
        public static ClanData Get(ulong id) => new ClanData { id = id };

        /// <summary>
        /// Retrieves a ClanData for the specified CSteamID.
        /// </summary>
        public static ClanData Get(CSteamID id) => new ClanData { id = id.m_SteamID };

        #region Boilerplate

        public readonly int CompareTo(CSteamID other)
        {
            return id.CompareTo(other.m_SteamID);
        }

        public readonly int CompareTo(ClanData other)
        {
            return id.CompareTo(other.id);
        }

        public readonly int CompareTo(ulong other)
        {
            return id.CompareTo(other);
        }

        public readonly override string ToString()
        {
            return id.ToString();
        }

        public readonly bool Equals(CSteamID other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(ClanData other)
        {
            return id.Equals(other.id);
        }

        public readonly bool Equals(ulong other)
        {
            return id.Equals(other);
        }

        public readonly override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(ClanData l, ClanData r) => l.id == r.id;
        public static bool operator !=(ClanData l, ClanData r) => l.id != r.id;
        public static bool operator ==(ClanData l, CSteamID r) => l.id == r.m_SteamID;
        public static bool operator !=(ClanData l, CSteamID r) => l.id != r.m_SteamID;
        public static bool operator ==(CSteamID l, ClanData r) => l.m_SteamID == r.id;
        public static bool operator !=(CSteamID l, ClanData r) => l.m_SteamID != r.id;
        public static bool operator <(ClanData l, ClanData r) => l.id < r.id;
        public static bool operator >(ClanData l, ClanData r) => l.id > r.id;
        public static bool operator <=(ClanData l, ClanData r) => l.id <= r.id;
        public static bool operator >=(ClanData l, ClanData r) => l.id >= r.id;
        public static implicit operator CSteamID(ClanData c) => c.SteamId;
        public static implicit operator ClanData(CSteamID id) => new ClanData { id = id.m_SteamID };
        public static implicit operator ulong(ClanData c) => c.id;
        public static implicit operator ClanData(ulong id) => new ClanData { id = id };
    #endregion
    }
}
#endif
