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
#if !DISABLESTEAMWORKS && STEAM_INSTALLED
using Steamworks;
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Typed wrapper for <see cref="UserAchievementStored_t"/> callback data.
    /// </summary>
    [Serializable]
    public struct UserAchievementStoredData
    {
        /// <summary>The game associated with the achievement.</summary>
        public GameData game;
        /// <summary>Indicates if it is a group achievement.</summary>
        public bool groupAchievement;
        /// <summary>The API name of the achievement.</summary>
        public string achievementName;
        /// <summary>The current progress toward unlocking the achievement.</summary>
        public uint currentProgress;
        /// <summary>The maximum progress required to unlock the achievement.</summary>
        public uint maxProgress;

        /// <summary>
        /// Initialises a new instance of the struct.
        /// </summary>
        public UserAchievementStoredData(GameData game, bool groupAchievement, string achievementName, uint currentProgress, uint maxProgress)
        {
            this.game = game;
            this.groupAchievement = groupAchievement;
            this.achievementName = achievementName;
            this.currentProgress = currentProgress;
            this.maxProgress = maxProgress;
        }

        /// <summary>
        /// Initialises a new instance of the struct from native Steamworks data.
        /// </summary>
        public UserAchievementStoredData(UserAchievementStored_t data)
        {
            game = data.m_nGameID;
            groupAchievement = data.m_bGroupAchievement;
            achievementName = data.m_rgchAchievementName;
            currentProgress = data.m_nCurProgress;
            maxProgress = data.m_nMaxProgress;
        }

        /// <summary>
        /// Implicit conversion from native <see cref="UserAchievementStored_t"/>.
        /// </summary>
        /// <param name="data">The native data.</param>
        public static implicit operator UserAchievementStoredData(UserAchievementStored_t data) => new(data);
    }
}
#endif
