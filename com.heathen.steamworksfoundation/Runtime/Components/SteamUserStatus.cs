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
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Monitors the status of a Steam user and updates UI elements accordingly.
    /// </summary>
    [ModularComponent(typeof(SteamUserData), "Status", nameof(settings))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    [HelpURL("https://heathen.group/kb/user/")]
    public class SteamUserStatus : MonoBehaviour
    {
        /// <summary>
        /// Defines the visual and textual representation for a specific user status.
        /// </summary>
        [Serializable]
        public class StatusReferences
        {
            /// <summary>The icon associated with this status.</summary>
            public Sprite icon;
            /// <summary>Should the icon color be updated when this status is applied?</summary>
            public bool   setIconColor;
            /// <summary>The color to apply to the icon.</summary>
            public Color  iconColor = Color.white;
            /// <summary>The message to display for this status. Use %gameName% to insert the name of the game the player is currently playing.</summary>
            [Tooltip("Use %gameName% to insert the name of the game the player is currently playing.")]
            public string message;
            /// <summary>Should the message color be updated when this status is applied?</summary>
            public bool   setMessageColor;
            /// <summary>The color to apply to the message label.</summary>
            public Color  messageColor = Color.white;

            /// <summary>
            /// Applies the status references to the provided image and label.
            /// </summary>
            /// <param name="image">The image to update.</param>
            /// <param name="label">The label to update.</param>
            public void Apply(Image image, TextMeshProUGUI label)
            {
                if (image != null)
                {
                    image.gameObject.SetActive(true);
                    image.sprite = icon;
                    if (setIconColor) image.color = iconColor;
                }
                if (label != null)
                {
                    label.text = message;
                    if (setMessageColor) label.color = messageColor;
                }
            }
        }

        /// <summary>
        /// A collection of status references for various Steam persona states.
        /// </summary>
        [Serializable]
        public class StatusOptions
        {
            /// <summary>The status to show when the user is in the same game as the local user.</summary>
            public StatusReferences inThisGame     = new() { message = "Playing this game" };
            /// <summary>The status to show when the user is in a different game.</summary>
            public StatusReferences inAnotherGame  = new() { message = "In another game" };
            /// <summary>The status to show when the user is online.</summary>
            public StatusReferences online         = new() { message = "Online" };
            /// <summary>The status to show when the user is offline.</summary>
            public StatusReferences offline        = new() { message = "Offline" };
            /// <summary>The status to show when the user is busy.</summary>
            public StatusReferences busy           = new() { message = "Busy" };
            /// <summary>The status to show when the user is away.</summary>
            public StatusReferences away           = new() { message = "Away" };
            /// <summary>The status to show when the user is in snooze mode.</summary>
            public StatusReferences snooze         = new() { message = "Snooze" };
            /// <summary>The status to show when the user is looking to trade.</summary>
            public StatusReferences lookingToTrade = new() { message = "Looking to Trade" };
            /// <summary>The status to show when the user is looking to play.</summary>
            public StatusReferences lookingToPlay  = new() { message = "Looking to Play" };
        }

        /// <summary>
        /// Configuration and UI element mappings for the user status component.
        /// </summary>
        [Serializable]
        public class StatusSettings
        {
            /// <summary>The status configuration to use.</summary>
            public StatusOptions configuration = new();
            /// <summary>The list of images to update based on user status.</summary>
            [Header("Elements")]
            public List<Image>            images = new();
            /// <summary>The list of labels to update based on user status.</summary>
            public List<TextMeshProUGUI>  labels = new();
        }

        /// <summary>
        /// The settings for this status component.
        /// </summary>
        public StatusSettings settings = new();

        private SteamUserData _mUserData;

        private void Awake()
        {
            _mUserData = GetComponent<SteamUserData>();
            _mUserData.onChanged.AddListener(HandlePersonaStateChange);
            SteamTools.Events.OnFriendRichPresenceUpdate += HandleRichPresenceUpdate;
        }

        private void OnDestroy()
        {
            if (_mUserData != null)
                _mUserData.onChanged.RemoveListener(HandlePersonaStateChange);
            SteamTools.Events.OnFriendRichPresenceUpdate -= HandleRichPresenceUpdate;
        }

        private void HandlePersonaStateChange(UserData user, EPersonaChange flag) => Refresh();
        private void HandleRichPresenceUpdate(UserData user, AppData app) => Refresh();

        /// <summary>
        /// Forces a refresh of the UI elements based on the current user status.
        /// </summary>
        public void Refresh()
        {
            int max = Mathf.Max(settings.images.Count, settings.labels.Count);
            for (int i = 0; i < max; i++)
            {
                Image           icon    = settings.images.Count > i ? settings.images[i] : null;
                TextMeshProUGUI message = settings.labels.Count > i ? settings.labels[i] : null;

                if (_mUserData.Data.GetGamePlayed(out var gameInfo))
                {
                    if (gameInfo.Game.IsMe)
                        settings.configuration.inThisGame.Apply(icon, message);
                    else
                        settings.configuration.inAnotherGame.Apply(icon, message);
                }
                else
                {
                    switch (_mUserData.Data.State)
                    {
                        case EPersonaState.k_EPersonaStateAway:
                            settings.configuration.away.Apply(icon, message);
                            break;
                        case EPersonaState.k_EPersonaStateBusy:
                            settings.configuration.busy.Apply(icon, message);
                            break;
                        case EPersonaState.k_EPersonaStateOnline:
                            settings.configuration.online.Apply(icon, message);
                            break;
                        case EPersonaState.k_EPersonaStateSnooze:
                            settings.configuration.snooze.Apply(icon, message);
                            break;
                        case EPersonaState.k_EPersonaStateLookingToPlay:
                            settings.configuration.lookingToPlay.Apply(icon, message);
                            break;
                        case EPersonaState.k_EPersonaStateLookingToTrade:
                            settings.configuration.lookingToTrade.Apply(icon, message);
                            break;
                        default:
                            settings.configuration.offline.Apply(icon, message);
                            break;
                    }
                }
            }
        }

    }
}
#endif
