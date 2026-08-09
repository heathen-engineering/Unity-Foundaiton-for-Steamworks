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
using System;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Named overlays that can be opened via <see cref="API.Overlay.Client.Activate(OverlayDialog)"/>.
    /// </summary>
    [Serializable]
    public enum OverlayDialog
    {
        /// <summary>Opens the friends dialogue.</summary>
        Friends,
        /// <summary>Opens the community dialogue.</summary>
        Community,
        /// <summary>Opens the players dialogue.</summary>
        Players,
        /// <summary>Opens the settings dialogue.</summary>
        Settings,
        /// <summary>Opens the official game group dialogue.</summary>
        Officalgamegroup,
        /// <summary>Opens the stats dialogue.</summary>
        Stats,
        /// <summary>Opens the achievements dialogue.</summary>
        Achievements,
    }
}
#endif
