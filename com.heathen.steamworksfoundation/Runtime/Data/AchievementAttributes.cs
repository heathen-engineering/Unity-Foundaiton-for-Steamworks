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

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the attributes of a Steam achievement used to retrieve metadata.
    /// </summary>
    public enum AchievementAttributes
    {
        /// <summary>Get the localised name of the achievement.</summary>
        Name,
        /// <summary>Get the localised description of the achievement.</summary>
        Desc,
        /// <summary>Returns "1" if the achievement is hidden from users before being unlocked.</summary>
        Hidden,
    }
}
#endif
