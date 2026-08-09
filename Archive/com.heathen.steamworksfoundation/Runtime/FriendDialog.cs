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
    /// Represents the various dialogue types that can be opened in the Steam Overlay.
    /// </summary>
    public enum FriendDialog
    {
        /// <summary>Opens the overlay web browser to the specified user or group's profile.</summary>
        Steamid,
        /// <summary>Opens a chat window to the specified user or joins the group chat.</summary>
        Chat,
        /// <summary>Opens a window for a Steam Trading session started via the ISteamEconomy/StartTrade Web API.</summary>
        Jointrade,
        /// <summary>Opens the overlay web browser to the specified user's stats.</summary>
        Stats,
        /// <summary>Opens the overlay web browser to the specified user's achievements.</summary>
        Achievements,
        /// <summary>Opens the overlay in minimal mode to add the target user as a friend.</summary>
        Friendadd,
        /// <summary>Opens the overlay in minimal mode to remove the target friend.</summary>
        Friendremove,
        /// <summary>Opens the overlay in minimal mode to accept an incoming friend invite.</summary>
        Friendrequestaccept,
        /// <summary>Opens the overlay in minimal mode to ignore an incoming friend invite.</summary>
        Friendrequestignore,
    }
}
#endif
