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
    /// Encapsulates information and operations related to a Steam App, including its unique ID,
    /// name retrieval, and interactions with the Steam Store. Provides data transformation
    /// between types such as AppId_t, CGameID, uint, and ulong, and supports equality
    /// comparisons and sorting for Steam App instances.
    /// </summary>
    [Serializable]
    public struct AppData : IEquatable<AppId_t>, IEquatable<CGameID>, IEquatable<uint>, IEquatable<ulong>,
        IEquatable<AppData>, IComparable<AppData>, IComparable<AppId_t>, IComparable<uint>, IComparable<ulong>
    {
        /// <summary>
        /// Represents the unique identifier for an application in the Steamworks ecosystem.
        /// </summary>
        [SerializeField]
        private uint id;

        /// <summary>
        /// Provides access to the current application's <see cref="AppData"/> information.
        /// </summary>
        public static AppData Me => API.App.Id;

        /// <summary>
        /// Retrieves the Steamworks application identifier as an object of type <see cref="Steamworks.AppId_t"/>.
        /// This represents the unique identifier associated with the application in the Steamworks system.
        /// </summary>
        public readonly AppId_t AppId => new AppId_t(id);

        /// <summary>
        /// Indicates whether the current instance of the application matches the application identified by the Steamworks API.
        /// </summary>
        public readonly bool IsMe => AppId == API.App.Id;

        /// <summary>
        /// Gets the application's Steam App ID as an unsigned 32-bit integer.
        /// </summary>
        public readonly uint Id => AppId.m_AppId;

        /// <summary>
        /// Retrieves the <see cref="AppData"/> instance representing this application.
        /// </summary>
        /// <returns>An <see cref="AppData"/> object that corresponds to the current application.</returns>
        public static AppData Get() => Me;

        /// <summary>
        /// Retrieves the current instance of <see cref="AppData"/> representing the executing app.
        /// </summary>
        /// <returns>The <see cref="AppData"/> instance representing the current app.</returns>
        public static AppData Get(CGameID gameId) => gameId;

        /// <summary>
        /// Retrieves and returns the current AppData instance for the application in execution.
        /// </summary>
        /// <returns>The <see cref="AppData"/> instance representing the application currently in use.</returns>
        public static AppData Get(ulong gameId) => gameId;

        /// <summary>
        /// Retrieves the AppData for the current application instance.
        /// </summary>
        /// <returns>The <see cref="AppData"/> instance representing the current application.</returns>
        public static AppData Get(uint appId) => appId;

        /// <summary>
        /// Returns the <see cref="AppData"/> instance associated with the current application.
        /// </summary>
        /// <returns>The <see cref="AppData"/> instance representing the current application.</returns>
        public static AppData Get(AppId_t appId) => appId;

        #region Boilerplate

        public readonly int CompareTo(AppData other)
        {
            return AppId.CompareTo(other.AppId);
        }

        public readonly int CompareTo(AppId_t other)
        {
            return AppId.CompareTo(other);
        }

        public readonly int CompareTo(ulong other)
        {
            return AppId.CompareTo(new CGameID(other).AppID());
        }

        public readonly int CompareTo(uint other)
        {
            return AppId.m_AppId.CompareTo(other);
        }

        public readonly override string ToString()
        {
            return AppId.ToString();
        }

        public readonly bool Equals(AppData other)
        {
            return AppId.Equals(other.AppId);
        }

        public readonly bool Equals(AppId_t other)
        {
            return AppId.Equals(other);
        }

        public readonly bool Equals(uint other)
        {
            return AppId.m_AppId.Equals(other);
        }

        public readonly bool Equals(CGameID other)
        {
            return AppId.Equals(other.AppID());
        }

        public readonly bool Equals(ulong other)
        {
            return AppId.Equals(new CGameID(other).AppID());
        }

        public readonly override bool Equals(object obj)
        {
            return AppId.m_AppId.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return AppId.GetHashCode();
        }

        public static bool operator ==(AppData l, AppData r) => l.AppId == r.AppId;
        public static bool operator ==(AppId_t l, AppData r) => l == r.AppId;
        public static bool operator ==(AppData l, AppId_t r) => l.AppId == r;
        public static bool operator !=(AppData l, AppData r) => l.AppId != r.AppId;
        public static bool operator !=(AppId_t l, AppData r) => l != r.AppId;
        public static bool operator !=(AppData l, AppId_t r) => l.AppId != r;

        public static implicit operator AppData(CGameID id) => new AppData { id = id.AppID().m_AppId };
        public static implicit operator uint(AppData c) => c.AppId.m_AppId;
        public static implicit operator AppData(ulong id) => new AppData { id = new CGameID(id).AppID().m_AppId };
        public static implicit operator AppData(uint id) => new AppData { id = id };
        public static implicit operator AppId_t(AppData c) => c.AppId;
        public static implicit operator AppData(AppId_t id) => new AppData { id = id.m_AppId };
        public static implicit operator AppData(GameData id) => new AppData { id = id.App.Id };
        #endregion
    }
}
#endif
