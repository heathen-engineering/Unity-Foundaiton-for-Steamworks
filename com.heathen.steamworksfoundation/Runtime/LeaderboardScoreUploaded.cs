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
    /// <summary>Wraps the native <see cref="LeaderboardScoreUploaded_t"/> callback result.</summary>
    [Serializable]
    public struct LeaderboardScoreUploaded
    {
        public LeaderboardScoreUploaded_t Data;
        public readonly bool         Success          => Data.m_bSuccess != 0;
        public readonly bool         ScoreChanged     => Data.m_bScoreChanged != 0;
        public readonly LeaderboardData Leaderboard   => Data.m_hSteamLeaderboard;
        public readonly int          Score            => Data.m_nScore;
        public readonly int          GlobalRankNew    => Data.m_nGlobalRankNew;
        public readonly int          GlobalRankPrevious => Data.m_nGlobalRankPrevious;

        public static implicit operator LeaderboardScoreUploaded(LeaderboardScoreUploaded_t native)   => new() { Data = native };
        public static implicit operator LeaderboardScoreUploaded_t(LeaderboardScoreUploaded heathen) => heathen.Data;
    }
}
#endif
