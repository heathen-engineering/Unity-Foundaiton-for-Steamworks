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
using Steamworks;
using System;
using UnityEngine;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// The <c>User</c> class provides functionality for interacting with the current user in Steamworks API.
    /// </summary>
    /// <remarks>
    /// This class offers access to user-related data such as the user's Steam ID, level, and rich presence.
    /// It also provides methods to interact with the user's state, phone verification, NAT status, and
    /// more through Steamworks client utilities.
    /// </remarks>
    public static class User
    {
        /// <summary>
        /// The <c>Client</c> class provides access to user-specific functionality within the Steamworks API.
        /// </summary>
        /// <remarks>
        /// This class enables interaction with the authenticated Steam user, including retrieving user information,
        /// managing the user's rich presence, checking the account security features such as two-factor authentication
        /// and phone verification, and advertising the user's game or server details.
        /// Additionally, it offers mechanisms to request store authentication URLs and query or modify the current user's rich presence data.
        /// </remarks>
        public static class Client
        {
            [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
            static void Init()
            {
                _mStoreAuthURLResponseT = null;
            }

            private static CallResult<StoreAuthURLResponse_t> _mStoreAuthURLResponseT;

            /// <summary>
            /// Represents the Steam ID of the user currently logged into the Steam client.
            /// This user is typically referred to as the 'local user' or 'current user'.
            /// </summary>
            public static UserData Id => SteamUser.GetSteamID();

            /// <summary>
            /// Retrieves the Steam level of the user currently logged into the Steam client.
            /// The Steam level is determined by the number of badges earned and other user milestones.
            /// </summary>
            public static int Level => SteamUser.GetPlayerSteamLevel();

            /// <summary>
            /// Represents the key-value pairs of rich presence data associated with the current user.
            /// Rich presence allows developers to set custom, game-specific strings that other users can view.
            /// This data is typically displayed in the user's profile or friend list on platforms that support it.
            /// </summary>
            public static StringKeyValuePair[] RichPresence
            {
                get 
                {
                    var count = SteamFriends.GetFriendRichPresenceKeyCount(SteamUser.GetSteamID());
                    var results = new StringKeyValuePair[count];
                    for (int i = 0; i < count; i++)
                    {
                        var key = SteamFriends.GetFriendRichPresenceKeyByIndex(SteamUser.GetSteamID(), i);
                        var value = SteamFriends.GetFriendRichPresence(SteamUser.GetSteamID(), key);
                        results[i] = new StringKeyValuePair { key = key, value = value };
                    }

                    return results;
                }
                set
                {
                    foreach(var entry in value)
                    {
                        SteamFriends.ClearRichPresence();
                        SteamFriends.SetRichPresence(entry.key, entry.value);
                    }
                }
            }

            /// <summary>
            /// Indicates whether the current user is behind a Network Address Translation (NAT) device.
            /// Returns true if the user is behind a NAT, and false otherwise.
            /// </summary>
            public static bool IsBehindNAT => SteamUser.BIsBehindNAT();

            /// <summary>
            /// Indicates whether the user's phone is in the process of being verified by Steam.
            /// This property reflects the intermediate state where the phone identification process has started
            /// but has not yet been completed or fully verified.
            /// </summary>
            public static bool IsPhoneIdentifying => SteamUser.BIsPhoneIdentifying();

            /// <summary>
            /// Indicates whether the user's phone number requires verification.
            /// Returns <c>true</c> if the phone number linked to the user's account
            /// needs to be verified; otherwise, <c>false</c>.
            /// </summary>
            public static bool IsPhoneRequiringVerification => SteamUser.BIsPhoneRequiringVerification();

            /// <summary>
            /// Indicates whether the user's phone number has been successfully verified.
            /// This status is determined through the linked Steam account's security settings.
            /// </summary>
            public static bool IsPhoneVerified => SteamUser.BIsPhoneVerified();

            /// <summary>
            /// Indicates whether the current user's Steam account has two-factor authentication enabled.
            /// </summary>
            public static bool IsTwoFactorEnabled => SteamUser.BIsTwoFactorEnabled();

            /// <summary>
            /// Indicates whether the user is currently logged into the Steam client.
            /// </summary>
            public static bool LoggedOn => SteamUser.BLoggedOn();

            /// <summary>
            /// Sets the rich presence data for an unsecured game server that the user is playing on. This data allows friends to view the game information and join the game.
            /// </summary>
            /// <param name="gameServerId">The Steam ID of the game server. Use k_steamIDNonSteamGS if setting the IP/Port, or k_steamIDNil if clearing the presence data.</param>
            /// <param name="ip">The IP address of the game server in host order.</param>
            /// <param name="port">The connection port of the game server in host order.</param>
            public static void AdvertiseGame(CSteamID gameServerId, uint ip, ushort port) =>
                SteamUser.AdvertiseGame(gameServerId, ip, port);

            /// <summary>
            /// Sets the rich presence data for a non-secured game server where the user is currently playing. This allows friends to view the server information and join the game.
            /// </summary>
            /// <param name="gameServerId">The Steam ID of the game server. Use k_steamIDNonSteamGS to specify the IP and port, or k_steamIDNil to clear the current presence data.</param>
            /// <param name="ip">The IP address of the game server in host order.</param>
            /// <param name="port">The connection port of the game server in host order.</param>
            public static void AdvertiseGame(CSteamID gameServerId, string ip, ushort port) =>
                SteamUser.AdvertiseGame(gameServerId, Utilities.IPStringToUint(ip), port);

            /// <summary>
            /// Gets the level of the users Steam badge for your game.
            /// </summary>
            /// <param name="series">If you only have one set of cards, the series will be 1.</param>
            /// <param name="foil">Check if they have received the foil badge.</param>
            /// <returns>The level of the badge, 0 if they don't have it.</returns>
            public static int GetGameBadgeLevel(int series, bool foil) => SteamUser.GetGameBadgeLevel(series, foil);
            /// <summary>
            /// Requests a URL which authenticates an in-game browser for store check-out and then redirects to the specified URL. As long as the in-game browser accepts and handles session cookies, Steam microtransaction checkout pages will automatically recognise the user instead of presenting a login page.
            /// </summary>
            /// <remarks>
            /// <para>
            /// NOTE: The URL has a very short lifetime to prevent history-snooping attacks, so you should only call this API when you are about to launch the browser, or else immediately navigate to the result URL using a hidden browser window.
            /// </para>
            /// <para>
            /// NOTE: The resulting authorisation cookie has an expiration time of one day, so it would be a good idea to request and visit a new auth URL every 12 hours.
            /// </para>
            /// </remarks>
            /// <param name="redirectUrl"></param>
            /// <param name="callback">Invoked by Steam when the request is resolved</param>
            public static void RequestStoreAuthURL(string redirectUrl, Action<StoreAuthURLResponse_t, bool> callback)
            {
                if (callback == null)
                    return;

                _mStoreAuthURLResponseT ??= CallResult<StoreAuthURLResponse_t>.Create();

                var handle = SteamUser.RequestStoreAuthURL(redirectUrl);
                _mStoreAuthURLResponseT.Set(handle, callback.Invoke);
            }

            /// <summary>
            /// Sets a key-value pair in the rich presence data for the currently logged-in user. This information can be viewed by others and allows for displaying custom status or data for the user's current activity.
            /// </summary>
            /// <param name="key">The key for the rich presence entry. This value identifies the type of data being stored (e.g. "status" or "game").</param>
            /// <param name="value">The value associated with the specified key. This represents the content to display for the rich presence entry.</param>
            /// <returns>True if the rich presence data was successfully set; otherwise, false.</returns>
            public static bool SetRichPresence(string key, string value) => SteamFriends.SetRichPresence(key, value);

            /// <summary>
            /// Clears all current rich presence key-value pairs associated with the local user's Steam account.
            /// This removes any status or data previously set using rich presence.
            /// </summary>
            public static void ClearRichPresence() => SteamFriends.ClearRichPresence();

            /// <summary>
            /// Retrieves the rich presence value associated with the specified key for the currently logged-in user.
            /// </summary>
            /// <param name="key">The key of the rich presence data to retrieve.</param>
            /// <returns>The value of the rich presence data associated with the specified key, or null if no value is set.</returns>
            public static string GetRichPresence(string key) => SteamFriends.GetFriendRichPresence(SteamUser.GetSteamID(), key);
        }
    }
}
#endif
