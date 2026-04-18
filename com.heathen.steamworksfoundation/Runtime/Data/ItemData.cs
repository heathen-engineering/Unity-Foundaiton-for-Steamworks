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
    /// Represents a Steam inventory item definition.
    /// </summary>
    [Serializable]
    public struct ItemData : IEquatable<ItemData>, IEquatable<int>, IEquatable<SteamItemDef_t>, IComparable<ItemData>, IComparable<int>, IComparable<SteamItemDef_t>
    {
        /// <summary>
        /// Represents the unique identifier for an item within the Steam Inventory system.
        /// </summary>
        public int id;

        /// <summary>
        /// Gets the <see cref="ItemData"/> representing the indicated item.
        /// </summary>
        public static ItemData Get(int id) => id;

        /// <summary>
        /// Get the <see cref="ItemData"/> based on the native <see cref="SteamItemDef_t"/>.
        /// </summary>
        public static ItemData Get(SteamItemDef_t id) => id;

        #region Boilerplate

        public readonly int CompareTo(ItemData other)
        {
            return id.CompareTo(other.id);
        }

        public readonly int CompareTo(int other)
        {
            return id.CompareTo(other);
        }

        public readonly int CompareTo(SteamItemDef_t other)
        {
            return id.CompareTo(other.m_SteamItemDef);
        }

        public readonly bool Equals(ItemData other)
        {
            return id.Equals(other.id);
        }

        public readonly bool Equals(int other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(SteamItemDef_t other)
        {
            return id.Equals(other.m_SteamItemDef);
        }

        public readonly override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(ItemData l, ItemData r) => l.id == r.id;
        public static bool operator ==(ItemData l, int r) => l.id == r;
        public static bool operator ==(ItemData l, SteamItemDef_t r) => l.id == r.m_SteamItemDef;
        public static bool operator !=(ItemData l, ItemData r) => l.id != r.id;
        public static bool operator !=(ItemData l, int r) => l.id != r;
        public static bool operator !=(ItemData l, SteamItemDef_t r) => l.id != r.m_SteamItemDef;

        public static implicit operator int(ItemData c) => c.id;
        public static implicit operator ItemData(int id) => new ItemData { id = id };
        public static implicit operator SteamItemDef_t(ItemData c) => new SteamItemDef_t(c.id);
        public static implicit operator ItemData(SteamItemDef_t id) => new ItemData { id = id.m_SteamItemDef };
        #endregion
    }
}
#endif
