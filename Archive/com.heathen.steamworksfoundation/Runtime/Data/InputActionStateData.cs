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
#if !DISABLESTEAMWORKS  && STEAM_INSTALLED

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents the current state of the data for a specific input action.
    /// </summary>
    [System.Serializable]
    public struct InputActionStateData
    {
        /// <summary>The name of the action this data relates to.</summary>
        public string name;
        /// <summary>The type of action this data relates to.</summary>
        public InputActionType type;
        /// <summary>The controller this data is from.</summary>
        public Steamworks.InputHandle_t controller;
        /// <summary>Whether or not this action is currently available to be bound in the active action set.</summary>
        public bool active;
        /// <summary>The native <see cref="Steamworks.EInputSourceMode"/> related to this action.</summary>
        public Steamworks.EInputSourceMode mode;
        /// <summary>The state of the action e.g. true if pressed, pulled, rocked or otherwise non-zero.</summary>
        public bool state;
        /// <summary>The value of the x axis if any.</summary>
        public float x;
        /// <summary>The value of the y axis if any.</summary>
        public float y;

        public override string ToString()
        {
            if (type == InputActionType.Analog)
            {
                if (active)
                    return "Active: X[" + x + "] Y[" + y + "]";
                else
                    return "Inactive";
            }
            else
            {
                if (active)
                    return "Active: " + (state ? "Engaged" : "Idle");
                else
                    return "Inactive";
            }
        }
    }
}
#endif
