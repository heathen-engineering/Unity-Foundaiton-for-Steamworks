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
#if !DISABLESTEAMWORKS && STEAM_INSTALLED
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Identifies a named Steam Input action and its type (analog or digital).
    /// Steam API calls (handle look-up, glyph retrieval, etc.) are provided by
    /// <c>InputActionDataExtensions</c> in the Toolkit package.
    /// </summary>
    [Serializable]
    public struct InputActionData
    {
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [SerializeField]
        private InputActionType type;

        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [SerializeField]
        private string name;

        /// <summary>Whether this action is analog or digital.</summary>
        public readonly InputActionType Type => type;

        /// <summary>The API name of this action as registered in Steam.</summary>
        public readonly string Name => name;

        /// <summary>Creates a new <see cref="InputActionData"/> for the named action.</summary>
        public InputActionData(string actionName, InputActionType actionType)
        {
            type = actionType;
            name = actionName;
        }
    }
}
#endif
