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
using System.Collections.Generic;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a structure for managing downloadable content (DLC) within the Steamworks integration.
    /// </summary>
    [Serializable]
    public struct DlcData : IEquatable<AppId_t>, IEquatable<uint>, IEquatable<AppData>, IComparable<AppData>,
        IComparable<AppId_t>, IComparable<uint>
    {
        private static readonly Dictionary<uint, (string name, bool available)> _cache = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() => _cache.Clear();

        /// <summary>
        /// The unique identifier representing the downloadable content (DLC).
        /// </summary>
        [SerializeField] private uint id;

        /// <summary>
        /// Represents the unique application identifier (App ID) for a downloadable content (DLC).
        /// </summary>
        public readonly AppId_t AppId => new AppId_t(id);

        /// <summary>
        /// Gets the unique identifier of the downloadable content (DLC).
        /// </summary>
        public readonly uint Id => id;

        /// <summary>
        /// Whether this DLC is currently marked as available, as reported by the last call to
        /// <c>SteamApps.BGetDLCDataByIndex</c>. Returns <c>false</c> if not yet cached.
        /// </summary>
        public readonly bool Available => _cache.TryGetValue(id, out var c) && c.available;

        /// <summary>
        /// The display name of this DLC, as reported by the last call to
        /// <c>SteamApps.BGetDLCDataByIndex</c>. Returns an empty string if not yet cached.
        /// </summary>
        public readonly string Name => _cache.TryGetValue(id, out var c) ? c.name : string.Empty;

        /// <summary>
        /// Constructs a <see cref="DlcData"/> with its id, availability, and name pre-cached.
        /// Typically called from <c>API.App.Client.Dlc</c> after reading
        /// <c>SteamApps.BGetDLCDataByIndex</c>.
        /// </summary>
        public DlcData(AppId_t appId, bool available, string name)
        {
            id = appId.m_AppId;
            _cache[id] = (name, available);
        }

        /// <summary>
        /// Retrieves a <see cref="DlcData"/> instance based on a specified application identifier.
        /// </summary>
        public static DlcData Get(uint appId) => appId;

        /// <summary>
        /// Retrieves a <see cref="DlcData"/> instance using the specified <see cref="AppId_t"/>.
        /// </summary>
        public static DlcData Get(AppId_t appId) => appId;

        /// <summary>
        /// Retrieves the <see cref="DlcData"/> associated with the given <see cref="AppData"/> object.
        /// </summary>
        public static DlcData Get(AppData appData) => appData.AppId;

        #region Boilerplate

        public override string ToString()
        {
            return id.ToString();
        }

        public readonly int CompareTo(AppData other)
        {
            return id.CompareTo(other.AppId.m_AppId);
        }

        public readonly int CompareTo(AppId_t other)
        {
            return id.CompareTo(other.m_AppId);
        }

        public readonly int CompareTo(uint other)
        {
            return id.CompareTo(other);
        }

        public readonly bool Equals(uint other)
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

        public readonly bool Equals(AppId_t other)
        {
            return id.Equals(other.m_AppId);
        }

        public readonly bool Equals(AppData other)
        {
            return id.Equals(other.AppId.m_AppId);
        }

        public static bool operator ==(DlcData l, DlcData r) => l.id == r.id;
        public static bool operator ==(DlcData l, AppData r) => l.id == r.Id;
        public static bool operator ==(DlcData l, AppId_t r) => l.id == r.m_AppId;
        public static bool operator ==(AppId_t l, DlcData r) => l.m_AppId == r.id;
        public static bool operator !=(DlcData l, DlcData r) => l.id != r.id;
        public static bool operator !=(DlcData l, AppData r) => l.id != r.AppId.m_AppId;
        public static bool operator !=(DlcData l, AppId_t r) => l.id != r.m_AppId;
        public static bool operator !=(AppId_t l, DlcData r) => l.m_AppId != r.id;

        public static implicit operator uint(DlcData c) => c.id;
        public static implicit operator DlcData(uint id) => new DlcData { id = id };
        public static implicit operator AppId_t(DlcData c) => c.AppId;
        public static implicit operator DlcData(AppId_t id) => new DlcData { id = id.m_AppId };
        public static implicit operator DlcData(AppData id) => new DlcData { id = id.Id };
        public static implicit operator AppData(DlcData id) => AppData.Get(id.id);
        #endregion
    }
}
#endif
