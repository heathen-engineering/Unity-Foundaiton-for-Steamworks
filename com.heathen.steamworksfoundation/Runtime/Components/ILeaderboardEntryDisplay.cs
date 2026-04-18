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

#if !DISABLESTEAMWORKS && STEAM_INSTALLED && !HEATHEN_TOOLKIT_INSTALLED
namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Implemented by components that display a single <see cref="LeaderboardEntry"/>.
    /// Used by <see cref="SteamLeaderboardDisplay"/> to populate instantiated entry prefabs.
    /// </summary>
    public interface ILeaderboardEntryDisplay
    {
        LeaderboardEntry Entry { get; set; }
    }
}
#endif
