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
    /// <summary>
    /// Represents a member of a Steam lobby.
    /// </summary>
    [Serializable]
    public struct LobbyMemberData : IEquatable<LobbyMemberData>
    {
        /// <summary>
        /// The lobby the user is a member of.
        /// </summary>
        public LobbyData lobby;

        /// <summary>
        /// The user that is a member of the lobby.
        /// </summary>
        public UserData user;

        /// <summary>
        /// Gets a <see cref="LobbyMemberData"/> for the given lobby and user.
        /// </summary>
        public static LobbyMemberData Get(LobbyData lobby, UserData user) => new LobbyMemberData() { lobby = lobby, user = user };

        /// <summary>
        /// Compares the current instance with another <see cref="LobbyMemberData"/>.
        /// </summary>
        /// <param name="other">The other member to compare to.</param>
        /// <returns>True if they represent the same user in the same lobby.</returns>
        public readonly bool Equals(LobbyMemberData other)
        {
            return other.lobby == lobby && other.user == user;
        }

        /// <summary>
        /// Compares the current instance with an object.
        /// </summary>
        /// <param name="obj">The object to compare to.</param>
        /// <returns>True if they are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(LobbyMemberData)) return false;
            else return Equals((LobbyMemberData)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code value.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(lobby, user);
        }

        /// <summary>
        /// Equality operator.
        /// </summary>
        /// <param name="l">Left operand.</param>
        /// <param name="r">Right operand.</param>
        /// <returns>True if equal.</returns>
        public static bool operator ==(LobbyMemberData l, LobbyMemberData r) => l.lobby == r.lobby && l.user == r.user;
        /// <summary>
        /// Inequality operator.
        /// </summary>
        /// <param name="l">Left operand.</param>
        /// <param name="r">Right operand.</param>
        /// <returns>True if not equal.</returns>
        public static bool operator !=(LobbyMemberData l, LobbyMemberData r) => l.lobby != r.lobby || l.user != r.user;
    }
}
#endif
