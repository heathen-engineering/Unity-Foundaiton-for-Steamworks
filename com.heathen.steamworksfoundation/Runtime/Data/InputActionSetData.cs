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
using Steamworks;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a Steam Input Action Set.
    /// </summary>
    [System.Serializable]
    public struct InputActionSetData : System.IEquatable<InputActionSetHandle_t>, System.IComparable<InputActionSetHandle_t>, System.IEquatable<ulong>, System.IComparable<ulong>
    {
        [SerializeField]
        private InputActionSetHandle_t handle;

        /// <summary>
        /// The primitive <see cref="ulong"/> value of the set's handle.
        /// </summary>
        public ulong Handle
        {
            get => handle.m_InputActionSetHandle;
        }

        /// <summary>
        /// Gets an <see cref="InputActionSetData"/> for the indicated action set handle.
        /// </summary>
        public static InputActionSetData Get(InputActionSetHandle_t handle)
        {
            return new InputActionSetData { handle = handle };
        }

        /// <summary>
        /// Gets an <see cref="InputActionSetData"/> for the indicated action set handle value.
        /// </summary>
        public static InputActionSetData Get(ulong handleValue)
        {
            return new InputActionSetData { handle = new InputActionSetHandle_t(handleValue) };
        }

        public bool Equals(InputActionSetHandle_t other)
        {
            return handle.Equals(other);
        }

        public int CompareTo(InputActionSetHandle_t other)
        {
            return handle.CompareTo(other);
        }

        public bool Equals(ulong other)
        {
            return handle.m_InputActionSetHandle.Equals(other);
        }

        public int CompareTo(ulong other)
        {
            return handle.m_InputActionSetHandle.CompareTo(other);
        }

        public override bool Equals(object obj)
        {
            return handle.Equals(obj);
        }

        public override int GetHashCode()
        {
            return handle.GetHashCode();
        }

        public static bool operator ==(InputActionSetData l, InputActionSetHandle_t r) => l.handle == r;
        public static bool operator ==(InputActionSetData l, ulong r) => l.handle == new InputActionSetHandle_t(r);
        public static bool operator ==(InputActionSetHandle_t l, InputActionSetData r) => l == r.handle;
        public static bool operator ==(ulong l, InputActionSetData r) => new InputActionSetHandle_t(l) == r.handle;
        public static bool operator !=(InputActionSetData l, InputActionSetHandle_t r) => l.handle != r;
        public static bool operator !=(InputActionSetData l, ulong r) => l.handle != new InputActionSetHandle_t(r);
        public static bool operator !=(InputActionSetHandle_t l, InputActionSetData r) => l != r.handle;
        public static bool operator !=(ulong l, InputActionSetData r) => new InputActionSetHandle_t(l) != r.handle;

        public static implicit operator ulong(InputActionSetData c) => c.handle.m_InputActionSetHandle;
        public static implicit operator InputActionSetData(ulong id) => Get(id);
        public static implicit operator InputActionSetHandle_t(InputActionSetData c) => c.handle;
        public static implicit operator InputActionSetData(InputActionSetHandle_t id) => Get(id);
    }
}
#endif
