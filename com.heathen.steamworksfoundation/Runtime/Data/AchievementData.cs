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
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Defines the structure for a Steam Achievement, providing information such as
    /// name, description, hidden status, achievement status, unlock time, global unlock percentage,
    /// and various methods for manipulating and querying the achievement state.
    /// </summary>
    [Serializable]
    public struct AchievementData : IEquatable<AchievementData>, IEquatable<string>, IComparable<AchievementData>, IComparable<string>
    {
        /// <summary>
        /// Represents the localised name of the achievement as it is displayed to the user.
        /// The value is determined based on the user's language preferences and the achievement's configuration.
        /// </summary>
        public readonly string Name =>
            API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(id, AchievementAttributes.Name);

        /// <summary>
        /// Provides the localised description of the achievement as configured in the Steamworks system.
        /// The description is determined based on the associated achievement's identifier and the user's language preferences.
        /// </summary>
        public readonly string Description =>
            API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(id, AchievementAttributes.Desc);

        /// <summary>
        /// Indicates whether the achievement is hidden from the user by default.
        /// A hidden achievement is not displayed to the user until it is unlocked.
        /// This property retrieves the value by checking the achievement's "Hidden" attribute
        /// through the Steamworks API.
        /// </summary>
        public readonly bool Hidden =>
            API.StatsAndAchievements.Client.GetAchievementDisplayAttribute(id, AchievementAttributes.Hidden) == "1";

        /// <summary>
        /// Represents the unique API name identifier associated with the achievement.
        /// This value serves as the internal key used by the Steamworks API to reference the achievement.
        /// </summary>
        public readonly string ApiName => id;

        /// <summary>
        /// The API Name as it appears in the Steamworks portal.
        /// </summary>
        [SerializeField]
        private string id;

        /// <summary>
        /// Indicates that this user has unlocked this achievement.
        /// </summary>
        /// <remarks>
        /// Only available on client builds
        /// </remarks>
        public readonly bool IsAchieved
        {
            get
            {
                if (API.StatsAndAchievements.Client.GetAchievement(id, out bool status))
                    return status;
                else
                    return false;
            }
            set
            {
                if (value)
                    API.StatsAndAchievements.Client.SetAchievement(id);
                else
                    API.StatsAndAchievements.Client.ClearAchievement(id);
            }
        }

        /// <summary>
        /// Represents the date and time when the achievement was unlocked.
        /// Returns a nullable <see cref="DateTime"/> value, where a valid <see cref="DateTime"/>
        /// indicates the unlocked time, and a null value means the achievement has not been unlocked.
        /// </summary>
        public readonly DateTime? UnlockTime
        {
            get
            {
                if (API.StatsAndAchievements.Client.GetAchievement(id, out _, out DateTime time))
                    return time;
                else
                    return null;
            }
        }

        /// <summary>
        /// Represents the percentage of global Steam users who have unlocked this achievement.
        /// This value is retrieved from the Steam statistics for the achievement and reflects the
        /// overall popularity or difficulty of achieving it across the platform.
        /// </summary>
        public readonly float GlobalPercent
        {
            get
            {
                API.StatsAndAchievements.Client.GetAchievementAchievedPercent(id, out var percent);
                return percent;
            }
        }

        /// <summary>
        /// Unlocks the achievement, marking it as achieved.
        /// </summary>
        public readonly void Unlock() => IsAchieved = true;

        /// <summary>
        /// Resets the unlocked status of the achievement, marking it as not achieved.
        /// </summary>
        public readonly void Clear() => IsAchieved = false;

        /// <summary>
        /// Unlocks the achievement for the specified user.
        /// </summary>
        /// <param name="user">The user who will have the achievement unlocked.</param>
        public readonly void Unlock(UserData user)
        {
            API.StatsAndAchievements.Server.SetUserAchievement(user, id);
        }

        /// <summary>
        /// Clears the specified achievement for the provided user.
        /// </summary>
        /// <remarks>
        /// This method is only available on server-side builds and does not function on client builds.
        /// </remarks>
        /// <param name="user">The user whose achievement is to be cleared.</param>
        public readonly void Clear(UserData user)
        {
            API.StatsAndAchievements.Server.ClearUserAchievement(user, id);
        }

        /// <summary>
        /// Retrieves the achievement status for a specified user.
        /// </summary>
        /// <param name="user">The user for whom the achievement status is being requested.</param>
        /// <returns>A boolean value indicating whether the achievement has been unlocked.</returns>
        public readonly bool GetAchievementStatus(UserData user)
        {
            API.StatsAndAchievements.Client.GetAchievement(user, id, out var achieved);
            return achieved;
        }

        /// <summary>
        /// Retrieves the unlocked state and unlock time of the achievement for a specified user.
        /// </summary>
        /// <param name="user">The user for whom the achievement's state and unlock time will be retrieved.</param>
        /// <returns>A tuple containing a boolean indicating whether the achievement is unlocked and a DateTime representing the unlock time.</returns>
        public readonly (bool unlocked, DateTime unlockTime) GetAchievementAndUnlockTime(UserData user)
        {
            API.StatsAndAchievements.Client.GetAchievement(user, id, out bool unlocked, out DateTime time);
            return (unlocked, time);
        }

        /// <summary>
        /// Retrieves the icon for the current achievement based on the logged-in user's progress.
        /// This method invokes the callback with the appropriate icon, returning either the locked or unlocked state.
        /// </summary>
        /// <param name="callback">A delegate to handle the result, receiving a <see cref="Texture2D"/> object representing the achievement icon.</param>
        public readonly void GetIcon(Action<Texture2D> callback) =>
            API.StatsAndAchievements.Client.GetAchievementIcon(id, callback);

        /// <summary>
        /// Requests the Steam client to store the current state of all statistics and achievements.
        /// </summary>
        public readonly void Store() => API.StatsAndAchievements.Client.StoreStats();

        /// <summary>
        /// Retrieves an achievement based on the provided API name.
        /// </summary>
        /// <param name="apiName">The API name of the achievement as specified in the Steam Developer Portal.</param>
        /// <returns>An <c>AchievementData</c> object corresponding to the provided API name.</returns>
        public static AchievementData Get(string apiName) => apiName;

        #region Boilerplate

        /// <summary>
        /// Returns a string representation of the achievement's API name.
        /// </summary>
        /// <returns>The API name of the achievement, or an empty string if the ID is null or empty.</returns>
        public readonly override string ToString()
        {
            return string.IsNullOrEmpty(id) ? string.Empty : id;
        }
        public readonly bool Equals(string other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(AchievementData other)
        {
            return id.Equals(other.id);
        }

        public readonly override bool Equals(object obj)
        {
            return id.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public readonly int CompareTo(AchievementData other)
        {
            return string.Compare(id, other.id, StringComparison.Ordinal);
        }

        public readonly int CompareTo(string other)
        {
            return string.Compare(id, other, StringComparison.Ordinal);
        }

        public static bool operator ==(AchievementData l, AchievementData r) => l.id == r.id;
        public static bool operator ==(string l, AchievementData r) => l == r.id;
        public static bool operator ==(AchievementData l, string r) => l.id == r;
        public static bool operator !=(AchievementData l, AchievementData r) => l.id != r.id;
        public static bool operator !=(string l, AchievementData r) => l != r.id;
        public static bool operator !=(AchievementData l, string r) => l.id != r;

        public static implicit operator string(AchievementData c) => string.IsNullOrEmpty(c.id) ? string.Empty : c.id;
        public static implicit operator AchievementData(string id) => new() { id = id };

        #endregion
    }
}
#endif
