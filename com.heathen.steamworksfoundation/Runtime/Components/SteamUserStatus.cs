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
    /// <summary>Monitors the status of a Steam user and updates UI elements accordingly.</summary>
    [ModularComponent(typeof(SteamUserData), "Status", nameof(settings))]
    [AddComponentMenu("")]
    [RequireComponent(typeof(SteamUserData))]
    public class SteamUserStatus : MonoBehaviour
    {
        [Serializable]
        public class StatusReferences
        {
            public Sprite icon;
            public bool   setIconColor;
            public Color  iconColor = Color.white;
            [Tooltip("Use %gameName% to insert the name of the game the player is currently playing.")]
            public string message;
            public bool   setMessageColor;
            public Color  messageColor = Color.white;

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

        [Serializable]
        public class StatusOptions
        {
            public StatusReferences inThisGame     = new() { message = "Playing this game" };
            public StatusReferences inAnotherGame  = new() { message = "In another game" };
            public StatusReferences online         = new() { message = "Online" };
            public StatusReferences offline        = new() { message = "Offline" };
            public StatusReferences busy           = new() { message = "Busy" };
            public StatusReferences away           = new() { message = "Away" };
            public StatusReferences snooze         = new() { message = "Snooze" };
            public StatusReferences lookingToTrade = new() { message = "Looking to Trade" };
            public StatusReferences lookingToPlay  = new() { message = "Looking to Play" };
        }

        [Serializable]
        public class StatusSettings
        {
            public StatusOptions configuration = new();
            [Header("Elements")]
            public List<Image>            images = new();
            public List<TextMeshProUGUI>  labels = new();
        }

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
