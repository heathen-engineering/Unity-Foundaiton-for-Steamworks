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
    /// <summary>
    /// Wraps the native <see cref="LeaderboardScoreUploaded_t"/> callback result.
    /// </summary>
    [Serializable]
    public struct LeaderboardScoreUploaded
    {
        /// <summary>The raw leaderboard score upload result data from Steamworks.</summary>
        public LeaderboardScoreUploaded_t Data;
        /// <summary>Indicates whether the score was successfully uploaded.</summary>
        public readonly bool         Success          => Data.m_bSuccess != 0;
        /// <summary>Indicates whether the user's score on the leaderboard changed.</summary>
        public readonly bool         ScoreChanged     => Data.m_bScoreChanged != 0;
        /// <summary>The leaderboard data associated with the upload.</summary>
        public readonly LeaderboardData Leaderboard   => Data.m_hSteamLeaderboard;
        /// <summary>The score that was uploaded.</summary>
        public readonly int          Score            => Data.m_nScore;
        /// <summary>The user's new global rank on the leaderboard.</summary>
        public readonly int          GlobalRankNew    => Data.m_nGlobalRankNew;
        /// <summary>The user's previous global rank on the leaderboard.</summary>
        public readonly int          GlobalRankPrevious => Data.m_nGlobalRankPrevious;

        /// <summary>
        /// Implicit conversion from native <see cref="LeaderboardScoreUploaded_t"/>.
        /// </summary>
        /// <param name="native">The native result.</param>
        public static implicit operator LeaderboardScoreUploaded(LeaderboardScoreUploaded_t native)   => new() { Data = native };
        /// <summary>
        /// Implicit conversion to native <see cref="LeaderboardScoreUploaded_t"/>.
        /// </summary>
        /// <param name="heathen">The Heathen wrapper.</param>
        public static implicit operator LeaderboardScoreUploaded_t(LeaderboardScoreUploaded heathen) => heathen.Data;
    }
}
#endif
