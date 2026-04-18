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
using Heathen.SteamworksIntegration.API;
using Steamworks;
using System;
using System.Collections.Generic;
// ReSharper disable PossiblyImpureMethodCallOnReadonlyVariable

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Represents a user and exposes key data in relation to that user
    /// </summary>
    [Serializable]
    public struct UserData : IEquatable<CSteamID>, IEquatable<ulong>, IEquatable<UserData>
    {
        /// <summary>
        /// Provides access to the <see cref="UserData"/> instance representing the local user.
        /// </summary>
        public static UserData Me => User.Client.Id;

        /// <summary>
        /// Retrieves an array of <see cref="UserData"/> instances representing the user's immediate friends.
        /// </summary>
        public static UserData[] MyFriends
        {
            get
            {
                var count = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
                var result = new UserData[count];
                for (int i = 0; i < count; i++)
                    result[i] = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                return result;
            }
        }

        /// <summary>
        /// Represents the unique identifier for a Steam user, encapsulated within a <see cref="CSteamID"/> structure.
        /// </summary>
        public CSteamID id;

        /// <summary>
        /// Indicates whether this <see cref="UserData"/> instance represents the local user.
        /// </summary>
        public readonly bool IsMe => id == User.Client.Id.id;

        /// <summary>
        /// Gets the 64-bit Steam identifier associated with the user. This identifier is unique across all Steam accounts.
        /// </summary>
        public readonly ulong SteamId => id.m_SteamID;

        /// <summary>
        /// Determines whether the current <see cref="UserData"/> instance is valid.
        /// </summary>
        /// <remarks>
        /// A valid <see cref="UserData"/> instance must meet the following criteria:
        /// 1. The underlying <see cref="CSteamID"/> is not nil.
        /// 2. The account type of the associated <see cref="CSteamID"/> is an individual account.
        /// 3. The associated <see cref="CSteamID"/> belongs to the public Steam universe.
        /// </remarks>
        /// <returns>
        /// Returns <c>true</c> if the current <see cref="UserData"/> instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public readonly bool IsValid
        {
            get
            {
                if (id == CSteamID.Nil
                    || id.GetEAccountType() != EAccountType.k_EAccountTypeIndividual
                    || id.GetEUniverse() != EUniverse.k_EUniversePublic)
                    return false;
                else
                    return true;
            }
        }

        /// <summary>
        /// Gets the display name of the user associated with this <see cref="UserData"/> instance.
        /// If the user is the local user, this is their current Steam Persona name.
        /// For other users, this represents their Steam Persona name as visible to the local user.
        /// </summary>
        public readonly string Name => SteamFriends.GetFriendPersonaName(id);

        /// <summary>
        /// Retrieves the user's preferred display name.
        /// Returns the custom nickname you have set for this user (if any), otherwise falls back to
        /// their Steam persona name. <see cref="SteamFriends.GetPlayerNickname"/> requires an active
        /// friendship and a nickname to have been set; it returns null/empty for non-friends or when
        /// no nickname exists, in which case the persona name is used.
        /// </summary>
        public readonly string Nickname
        {
            get
            {
                var nickname = SteamFriends.GetPlayerNickname(id);
                return string.IsNullOrEmpty(nickname) ? SteamFriends.GetFriendPersonaName(id) : nickname;
            }
        }

        /// <summary>
        /// Retrieves the current persona state of the user represented by this instance.
        /// The persona state indicates the user's activity or availability, such as Online, Offline, Away, or Busy.
        /// </summary>
        public readonly EPersonaState State => SteamFriends.GetFriendPersonaState(id);

        /// <summary>
        /// Indicates whether the user is currently playing a game.
        /// </summary>
        public readonly bool InGame => SteamFriends.GetFriendGamePlayed(id, out _);

        /// <summary>
        /// Indicates whether the friend represented by this instance is currently playing the same game as the local user.
        /// </summary>
        public readonly bool InThisGame => SteamFriends.GetFriendGamePlayed(id, out var friendInfo) && friendInfo.m_gameID == GameData.Me;

        /// <summary>
        /// Provides information about the game session in which the user is currently participating.
        /// </summary>
        public readonly FriendGameInfo GameInfo
        {
            get
            {
                SteamFriends.GetFriendGamePlayed(id, out var result);
                return result;
            }
        }

        /// <summary>
        /// Retrieves the Steam level of the user associated with this <see cref="UserData"/> instance.
        /// </summary>
        /// <remarks>
        /// The Steam level represents a user's account level on the Steam platform,
        /// which is determined by their overall activity and engagement.
        /// This value is fetched using the Steamworks API.
        /// </remarks>
        public readonly int Level => SteamFriends.GetFriendSteamLevel(id);

        /// <summary>
        /// Retrieves the unique account identifier associated with the user in the form of an <see cref="AccountID_t"/>.
        /// This value can be used to uniquely represent the user's Steam account within the Steamworks API.
        /// </summary>
        public readonly AccountID_t AccountId => id.GetAccountID();

        /// <summary>
        /// Gets the unique numeric identifier for a friend account, derived from the associated <see cref="AccountID_t"/>.
        /// </summary>
        public readonly uint FriendId => AccountId.m_AccountID;

        /// <summary>
        /// Retrieves the Steam friend's unique identifier represented as a hexadecimal string.
        /// </summary>
        public readonly string HexId => FriendId.ToString("X");

        /// <summary>
        /// Retrieves the history of persona names associated with the user.
        /// </summary>
        public readonly string[] NameHistory
        {
            get
            {
                var names = new List<string>();
                string name;
                int index = 0;
                while (!string.IsNullOrEmpty(name = SteamFriends.GetFriendPersonaNameHistory(id, index++)))
                    names.Add(name);
                return names.ToArray();
            }
        }

        /// <summary>
        /// Retrieves information about the game the user is currently playing.
        /// </summary>
        /// <param name="gameInfo">An output parameter that will be populated with the game information if the user is playing a game.</param>
        /// <returns>Returns true if the user is currently playing a game and the information was successfully retrieved; otherwise, returns false.</returns>
        public readonly bool GetGamePlayed(out FriendGameInfo gameInfo)
        {
            var result = SteamFriends.GetFriendGamePlayed(id, out FriendGameInfo_t raw);
            gameInfo = raw;
            return result;
        }

        /// <summary>
        /// Sends an invitation to the specified user to join the game using the given connection string.
        /// </summary>
        /// <param name="connectString">The connection string for the game session to which the user is being invited.</param>
        public readonly void InviteToGame(string connectString) => SteamFriends.InviteUserToGame(id, connectString);

        /// <summary>
        /// Sends a chat message to the specified user.
        /// </summary>
        /// <param name="message">The message to send to the user.</param>
        /// <returns>True if the message was successfully sent; otherwise, false.</returns>
        public readonly bool SendMessage(string message) => SteamFriends.ReplyToFriendMessage(id, message);

        /// <summary>
        /// Requests the persona name and avatar of a specified user.
        /// </summary>
        /// <returns>
        /// true if the information is being requested, with a PersonaStateChange_t callback to be posted upon retrieval;
        /// false if all details about the user are already available and can be used immediately.
        /// </returns>
        public readonly bool RequestInformation() => SteamFriends.RequestUserInformation(id, false);

        /// <summary>
        /// Retrieves the rich presence value associated with the specified key from the user's Steam profile.
        /// </summary>
        /// <param name="key">The rich presence key whose value is to be retrieved.</param>
        /// <returns>The string value associated with the specified rich presence key.</returns>
        public readonly string GetRichPresenceValue(string key) => SteamFriends.GetFriendRichPresence(id, key);

        /// <summary>
        /// Opens the Steam Overlay's Add Friend dialogue for the specified user, allowing the local user to send a friend request.
        /// </summary>
        public readonly void AddFriend() => AddFriend(this);

        /// <summary>
        /// Opens the Overlay Remove Friend dialogue to remove this user as the local user's friend
        /// </summary>
        public readonly void RemoveFriend() => RemoveFriend(this);

        /// <summary>
        /// Marks the specified user as someone the current player has played with recently.
        /// This is a notification typically used to help track recent in-game interactions.
        /// </summary>
        public readonly void SetPlayedWith() => SteamFriends.SetPlayedWith(id);

        /// <summary>
        /// Retrieves the status of an achievement for the specified user, including whether it has been unlocked and the time it was unlocked.
        /// </summary>
        /// <param name="achievement">The achievement data to query.</param>
        /// <returns>A tuple indicating whether the achievement is unlocked and the time it was unlocked, if available.</returns>
        public readonly (bool unlocked, DateTime unlockTime) GetAchievement(AchievementData achievement) => achievement.GetAchievementAndUnlockTime(this);

        /// <summary>
        /// Attempts to unlock the specified achievement for the current user.
        /// </summary>
        /// <param name="achievement">The achievement data representing the achievement to be unlocked.</param>
        /// <returns>True if the achievement was successfully set, false otherwise.</returns>
        public readonly bool SetAchievement(AchievementData achievement) => StatsAndAchievements.Client.SetAchievement(achievement);

        /// <summary>
        /// Clears the rich presence data for the local user, removing any previously set rich presence information.
        /// </summary>
        public static void ClearRichPresence() => SteamFriends.ClearRichPresence();

        /// <summary>
        /// Retrieves the UserData instance based on the given hexadecimal account ID.
        /// </summary>
        /// <param name="accountId">The hexadecimal string representing the user's account ID.</param>
        /// <returns>The UserData associated with the specified account ID. Returns an empty UserData instance if the ID is invalid.</returns>
        public static UserData Get(string accountId)
        {
            var id = Convert.ToUInt32(accountId, 16);
            if (id > 0)
                return Get(id);
            else
                return CSteamID.Nil;
        }

        /// <summary>
        /// Retrieves a UserData instance corresponding to a given Steam ID.
        /// </summary>
        /// <param name="id">The 64-bit Steam ID (ulong) to create a UserData instance for.</param>
        /// <returns>A UserData instance associated with the provided Steam ID.</returns>
        public static UserData Get(ulong id) => new UserData { id = new CSteamID(id) };

        /// <summary>
        /// Retrieves a <see cref="UserData"/> instance corresponding to the specified <see cref="CSteamID"/>.
        /// </summary>
        /// <param name="id">The Steam ID of the user to retrieve as a <see cref="UserData"/> object.</param>
        /// <returns>A <see cref="UserData"/> instance representing the specified Steam user.</returns>
        public static UserData Get(CSteamID id) => new UserData { id = id };

        /// <summary>
        /// Retrieves the current user's Steam user data.
        /// </summary>
        /// <returns>The <see cref="UserData"/> representing the current user.</returns>
        public static UserData Get() => User.Client.Id;

        /// <summary>
        /// Retrieves the UserData associated with the given account ID.
        /// </summary>
        /// <param name="accountId">The unique account ID represented as an uint.</param>
        /// <returns>A UserData instance corresponding to the provided account ID.</returns>
        public static UserData Get(uint accountId) => Get(new AccountID_t(accountId));

        /// <summary>
        /// Retrieves a user data instance based on the provided account ID.
        /// </summary>
        /// <param name="accountId">The account ID used to generate a UserData instance.</param>
        /// <returns>A UserData instance associated with the specified account ID.</returns>
        public static UserData Get(AccountID_t accountId) => new CSteamID(accountId, EUniverse.k_EUniversePublic, EAccountType.k_EAccountTypeIndividual);

        /// <summary>
        /// Sets the rich presence for the current user on Steam.
        /// </summary>
        /// <param name="key">The key identifying the field to update in rich presence.</param>
        /// <param name="value">The value to associate with the specified key.</param>
        /// <returns>Returns true if the rich presence was successfully updated; otherwise, false.</returns>
        public static bool SetRichPresence(string key, string value) => SteamFriends.SetRichPresence(key, value);

        /// <summary>
        /// Opens the Steam Overlay to the Add Friend dialogue, allowing the user to send a friend request to another user.
        /// </summary>
        /// <param name="friendId">The Steam ID of the user to whom the friend request will be sent, provided as a string.</param>
        /// <returns>True if the Steam ID was successfully parsed and the dialogue opened, false otherwise.</returns>
        public static bool AddFriend(string friendId)
        {
            if (uint.TryParse(friendId, out var friend))
            {
                AddFriend(friend);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Opens the Steam Overlay to display the Add Friend dialogue for the specified user.
        /// </summary>
        /// <param name="friendId">The Steam ID of the user to add as a friend.</param>
        public static void AddFriend(uint friendId) => AddFriend(Get(friendId));

        /// <summary>
        /// Opens the Steam Overlay to the "Add Friend" dialogue for the specified user.
        /// </summary>
        /// <param name="user">The user data of the individual to whom a friend request will be sent.</param>
        public static void AddFriend(UserData user) =>
            SteamFriends.ActivateGameOverlayToUser(FriendDialog.Friendadd.ToString(), user.id);

        /// <summary>
        /// Opens the Steam Overlay to the Add Friend dialogue for the specified user.
        /// </summary>
        /// <param name="user">The AccountID of the user to whom the Add Friend dialogue will be displayed.</param>
        public static void AddFriend(AccountID_t user) =>
            SteamFriends.ActivateGameOverlayToUser(FriendDialog.Friendadd.ToString(), Get(user).id);

        /// <summary>
        /// Opens the overlay to the "Remove Friend" dialogue for a specified user.
        /// </summary>
        /// <param name="user">The user to be removed from the friend list.</param>
        public static void RemoveFriend(UserData user) =>
            SteamFriends.ActivateGameOverlayToUser(FriendDialog.Friendremove.ToString(), user.id);

        /// <summary>
        /// Opens the overlay displaying the "Remove Friend" dialogue for the specified user.
        /// </summary>
        /// <param name="user">The account ID of the user to be removed as a friend.</param>
        public static void RemoveFriend(AccountID_t user) =>
            SteamFriends.ActivateGameOverlayToUser(FriendDialog.Friendremove.ToString(), Get(user).id);

        #region Boilerplate

        public readonly int CompareTo(UserData other)
        {
            return id.CompareTo(other.id);
        }

        public readonly int CompareTo(CSteamID other)
        {
            return id.CompareTo(other);
        }

        public readonly int CompareTo(ulong other)
        {
            return id.m_SteamID.CompareTo(other);
        }

        public readonly override string ToString()
        {
            return HexId;
        }

        public readonly bool Equals(UserData other)
        {
            return id.Equals(other.id);
        }

        public readonly bool Equals(CSteamID other)
        {
            return id.Equals(other);
        }

        public readonly bool Equals(ulong other)
        {
            return id.m_SteamID.Equals(other);
        }

        public readonly override bool Equals(object obj)
        {
            return id.m_SteamID.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public static bool operator ==(UserData l, UserData r) => l.id == r.id;
        public static bool operator ==(CSteamID l, UserData r) => l == r.id;
        public static bool operator ==(UserData l, CSteamID r) => l.id == r;
        public static bool operator !=(UserData l, UserData r) => l.id != r.id;
        public static bool operator !=(CSteamID l, UserData r) => l != r.id;
        public static bool operator !=(UserData l, CSteamID r) => l.id != r;

        public static implicit operator ulong(UserData c) => c.id.m_SteamID;
        public static implicit operator UserData(ulong id) => new() { id = new CSteamID(id) };
        public static implicit operator CSteamID(UserData c) => c.id;
        public static implicit operator UserData(CSteamID id) => new() { id = id };
    #endregion
    }
}
#endif
