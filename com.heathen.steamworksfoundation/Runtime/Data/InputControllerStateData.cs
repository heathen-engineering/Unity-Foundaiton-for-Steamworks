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

using System.Linq;
using Unity.Mathematics;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a controller's current data state.
    /// </summary>
    [System.Serializable]
    public struct InputControllerStateData
    {
        /// <summary>The handle of the controller this data represents.</summary>
        public Steamworks.InputHandle_t handle;
        /// <summary>A collection of all the inputs related to this controller and their current state.</summary>
        public InputActionStateData[] inputs;
        /// <summary>A collection of all the changes to input state since the last update of this controller.</summary>
        public InputActionUpdate[] changes;

        /// <summary>Gets the action data of a specific input action by name.</summary>
        public InputActionStateData GetActionData(string name) => inputs.FirstOrDefault(p => p.name == name);

        /// <summary>Check if the indicated action is active.</summary>
        public bool GetActive(string name) => inputs.FirstOrDefault(p => p.name == name).active;

        /// <summary>Check for the state of the indicated action.</summary>
        public bool GetState(string name) => inputs.FirstOrDefault(p => p.name == name).state;

        /// <summary>Gets the float value of this action.</summary>
        public float GetFloat(string name) => inputs.FirstOrDefault(p => p.name == name).x;

        /// <summary>Gets the <see cref="float2"/> value of this action.</summary>
        public float2 GetFloat2(string name)
        {
            var data = inputs.FirstOrDefault(p => p.name == name);
            return new float2(data.x, data.y);
        }

        /// <summary>Get the <see cref="Steamworks.EInputSourceMode"/> of the indicated action.</summary>
        public Steamworks.EInputSourceMode GetMode(string name) => inputs.FirstOrDefault(p => p.name == name).mode;
    }
}
#endif
