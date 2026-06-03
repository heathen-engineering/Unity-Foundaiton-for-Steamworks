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
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Common interface implemented by both Foundation's and Toolkit's
    /// <c>SteamLeaderboardDataEvents</c> components. Child leaderboard display
    /// components reference this interface so they compile regardless of which
    /// version of the events component is present.
    /// </summary>
    public interface ISteamLeaderboardDataEvents
    {
        /// <summary>Event invoked when the leaderboard data changes.</summary>
        UnityEvent onChange { get; }
        /// <summary>Event invoked when a leaderboard is found or created.</summary>
        UnityEvent onFindOrCreate { get; }
        /// <summary>Event invoked when a leaderboard find or create operation fails.</summary>
        UnityEvent onFindOrCreateFailure { get; }
        /// <summary>Event invoked when the local user's rank on the leaderboard changes.</summary>
        UnityEvent<LeaderboardScoreUploaded> onRankChanged { get; }
    }
}
#endif
