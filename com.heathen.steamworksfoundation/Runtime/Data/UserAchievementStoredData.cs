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
        public GameData game;
        public bool groupAchievement;
        public string achievementName;
        public uint currentProgress;
        public uint maxProgress;

        public UserAchievementStoredData(GameData game, bool groupAchievement, string achievementName, uint currentProgress, uint maxProgress)
        {
            this.game = game;
            this.groupAchievement = groupAchievement;
            this.achievementName = achievementName;
            this.currentProgress = currentProgress;
            this.maxProgress = maxProgress;
        }

        public UserAchievementStoredData(UserAchievementStored_t data)
        {
            game = data.m_nGameID;
            groupAchievement = data.m_bGroupAchievement;
            achievementName = data.m_rgchAchievementName;
            currentProgress = data.m_nCurProgress;
            maxProgress = data.m_nMaxProgress;
        }

        public static implicit operator UserAchievementStoredData(UserAchievementStored_t data) => new(data);
    }
}
#endif
