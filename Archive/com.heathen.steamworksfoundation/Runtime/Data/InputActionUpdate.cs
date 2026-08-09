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
using System;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public struct InputActionUpdate
    {
        public bool IsNil => string.IsNullOrEmpty(name);
        public Steamworks.InputHandle_t controller;
        public string name;
        public InputActionType type;
        public Steamworks.EInputSourceMode mode;

        public bool isActive;
        public bool isState;
        public float isX;
        public float isY;
        public bool wasActive;
        public bool wasState;
        public float wasX;
        public float wasY;
        public float DeltaX => isX - wasX;
        public float DeltaY => isY - wasY;
        public bool Active => isActive;
        public bool State => isState;
        public float X => isX;
        public float Y => isY;
        public InputActionStateData Data => new InputActionStateData
        {
            controller = controller,
            name = name,
            type = type,
            mode = mode,
            active = isActive,
            state = isState,
            x = isX,
            y = isY,
        };
    }
}
#endif
