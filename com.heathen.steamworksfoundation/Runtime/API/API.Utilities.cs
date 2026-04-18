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
using System.Net;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

namespace Heathen.SteamworksIntegration.API
{
    /// <summary>
    /// Provides utility methods for operations like IP address conversions,
    /// image buffer manipulations, and string token searches.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Converts an IP address in string format to its 32-bit unsigned integer representation.
        /// </summary>
        /// <param name="address">A string representing the IP address to be converted. The string must be parseable by System.Net.IPAddress.Parse.</param>
        /// <returns>A 32-bit unsigned integer representing the octet order of the IP address from left to right.</returns>
        public static uint IPStringToUint(string address)
        {
            var ipBytes = IPStringToBytes(address);
            var ip = (uint)ipBytes[0] << 24;
            ip += (uint)ipBytes[1] << 16;
            ip += (uint)ipBytes[2] << 8;
            ip += ipBytes[3];
            return ip;
        }

        /// <summary>
        /// Converts a 32-bit unsigned integer representing an IP address into its human-readable string representation.
        /// </summary>
        /// <param name="address">A 32-bit unsigned integer where each byte represents an octet in the order of byte 24, byte 16, byte 8, byte 0.</param>
        /// <returns>A string representing the IP address in standard dotted-decimal notation (e.g. "192.168.0.1").</returns>
        public static string IPUintToString(uint address)
        {
            var ipBytes = BitConverter.GetBytes(address);
            var ipBytesRevert = new byte[4];
            ipBytesRevert[0] = ipBytes[3];
            ipBytesRevert[1] = ipBytes[2];
            ipBytesRevert[2] = ipBytes[1];
            ipBytesRevert[3] = ipBytes[0];
            return new IPAddress(ipBytesRevert).ToString();
        }

        /// <summary>
        /// Converts an IP address in string format to its byte array representation.
        /// </summary>
        /// <param name="address">A string representing the IP address to be converted. The string must be parseable by System.Net.IPAddress.Parse.</param>
        /// <returns>A byte array containing the IP address in octet order from left to right as seen in the string (index 0, 1, 2, 3).</returns>
        public static byte[] IPStringToBytes(string address)
        {
            var ipAddress = IPAddress.Parse(address);
            return ipAddress.GetAddressBytes();
        }

        /// <summary>
        /// Flips a given image buffer vertically. This is commonly used when working with images loaded from Steamworks,
        /// as they are often inverted compared to the orientation Unity expects.
        /// </summary>
        /// <param name="width">The width of the image in pixels.</param>
        /// <param name="height">The height of the image in pixels.</param>
        /// <param name="buffer">The byte array containing the image data to be flipped. Each pixel is assumed to be represented by 4 bytes (e.g. RGBA format).</param>
        /// <returns>A new byte array containing the vertically flipped image data.</returns>
        public static byte[] FlipImageBufferVertical(int width, int height, byte[] buffer)
        {
            byte[] result = new byte[buffer.Length];

            int xWidth = width * 4;
            int yHeight = height;

            for (int y = 0; y < yHeight; y++)
            {
                for (int x = 0; x < xWidth; x++)
                {
                    result[x + ((yHeight - 1 - y) * xWidth)] = buffer[x + (xWidth * y)];
                }
            }

            return result;
        }

        /// <summary>
        /// Searches for a token within an input string that is enclosed by specified opening and closing patterns.
        /// </summary>
        /// <param name="openPattern">The string pattern that represents the opening boundary of the token.</param>
        /// <param name="closePattern">The string pattern that represents the closing boundary of the token.</param>
        /// <param name="input">The string to search for the token.</param>
        /// <param name="result">When the method returns, contains the token found within the boundaries of the specified patterns, or an empty string if no token is found.</param>
        /// <returns>A boolean value indicating whether a token was successfully found and extracted.</returns>
        public static bool FindToken(string openPattern, string closePattern, string input, out string result)
        {
            if(input.Contains(openPattern) && input.Contains(closePattern))
            {
                var lastOpenIndex = input.LastIndexOf(closePattern, StringComparison.Ordinal);
                var afterLastCloseIndex = input.IndexOf(closePattern, lastOpenIndex, StringComparison.Ordinal);
                if (afterLastCloseIndex != -1)
                {
                    result = input.Substring(lastOpenIndex, (afterLastCloseIndex - lastOpenIndex) + 1);
                    return true;
                }
                else
                {
                    result = string.Empty;
                    return false;
                }
            }
            else
            {
                result = string.Empty;
                return false;
            }
        }

        /// <summary>
        /// Provides a set of utility methods and properties to interact with the Steamworks client,
        /// offering access to Steam's runtime environment, user interface, and system-level features.
        /// </summary>
        public static class Client
        {
            /// <summary>
            /// Gets the 2-digit ISO 3166-1-alpha-2 format country code based on the client’s IP location.
            /// This property utilises an IP-to-location database to determine the geographical location of the client.
            /// For example, it may return values like "US" for the United States or "UK" for the United Kingdom.
            /// </summary>
            public static string IpCountry => SteamUtils.GetIPCountry();

            /// <summary>
            /// Gets the number of seconds that have elapsed since the app became active.
            /// This property provides a measure of the app's runtime in its current active state,
            /// which can be useful for tracking session durations or triggering time-based behaviours.
            /// </summary>
            public static uint SecondsSinceAppActive => SteamUtils.GetSecondsSinceAppActive();

            /// <summary>
            /// Gets the current server time as a DateTime object, representing the number of seconds elapsed since January 1, 1970, GMT (Unix time).
            /// This property is useful for synchronising time-sensitive operations with the Steam server, providing a consistent and reliable reference point.
            /// </summary>
            public static DateTime ServerRealTime => new DateTime(1970, 1, 1).AddSeconds(SteamUtils.GetServerRealTime());

            /// <summary>
            /// Gets the language in which the Steam client interface is currently displayed.
            /// This property retrieves the language setting configured in the Steam client itself,
            /// which may be useful for special cases where the application's behaviour needs to align
            /// with Steam's UI language selection. For most game-specific localisation needs, consider
            /// using appropriate game-specific language settings instead.
            /// </summary>
            public static string SteamUILanguage => SteamUtils.GetSteamUILanguage();

            /// <summary>
            /// Indicates whether Steam and the Steam Overlay are operating in Big Picture mode.
            /// This property can be used to detect if the Steam client is being utilised in a mode
            /// suitable for controller-friendly user interfaces, such as on a TV or similar setup.
            /// </summary>
            public static bool IsSteamInBigPictureMode => SteamUtils.IsSteamInBigPictureMode();

            /// <summary>
            /// Indicates whether the Steam client is currently operating in VR mode.
            /// Use this property to check if Steam's runtime environment is configured for virtual reality applications.
            /// </summary>
            public static bool IsSteamRunningInVR => SteamUtils.IsSteamRunningInVR();

            /// <summary>
            /// Indicates whether the Steam client is currently running on a Steam Deck device.
            /// Use this property to determine if the current environment is a Steam Deck,
            /// enabling you to adjust settings or behaviours specific to that platform.
            /// </summary>
            public static bool IsSteamRunningOnSteamDeck => SteamUtils.IsSteamRunningOnSteamDeck();

            /// <summary>
            /// Gets or sets a value indicating whether the VR headset content is being streamed
            /// through Steam Remote Play. Use this property to enable or disable VR headset
            /// streaming functionality based on runtime requirements.
            /// </summary>
            public static bool IsVRHeadsetStreamingEnabled
            {
                get => SteamUtils.IsVRHeadsetStreamingEnabled();
                set => SteamUtils.SetVRHeadsetStreamingEnabled(value);
            }

            /// <summary>
            /// Configures Steam Input to translate controller input into mouse and keyboard input for navigating a game launcher interface.
            /// </summary>
            /// <param name="mode">A boolean indicating whether to enable or disable the game launcher mode. Set to true to enable, or false to disable.</param>
            public static void SetGameLauncherMode(bool mode) => SteamUtils.SetGameLauncherMode(mode);

            /// <summary>
            /// Requests the Steam client to display the OpenVR dashboard within a virtual reality environment.
            /// </summary>
            public static void StartVRDashboard() => SteamUtils.StartVRDashboard();

            /// <summary>
            /// Opens a virtual keyboard overlay and sends keyboard input directly to the game.
            /// The position and size of the text input field are specified in pixels relative to the game window,
            /// allowing the virtual keyboard to be displayed in a position that avoids covering the text input field.
            /// </summary>
            /// <param name="mode">The type of virtual keyboard to open, such as single-line or multi-line text entry.</param>
            /// <param name="fieldPosition">The position in pixels of the text input field relative to the game window origin.</param>
            /// <param name="fieldSize">The size in pixels of the text input field to help position the virtual keyboard.</param>
            /// <returns>A boolean value indicating whether the virtual keyboard was successfully displayed.</returns>
            public static bool ShowVirtualKeyboard(EFloatingGamepadTextInputMode mode, int2 fieldPosition,
                int2 fieldSize)
            {
                if (SteamUtils.ShowFloatingGamepadTextInput(mode, fieldPosition.x, fieldPosition.y, fieldSize.x,
                        fieldSize.y))
                {
                    SteamTools.Events.InvokeOnKeyboardShown();
                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Displays a virtual keyboard interface suitable for gamepad input.
            /// </summary>
            /// <param name="mode">The input mode of the virtual keyboard, such as single-line, multi-line, email, or numeric.</param>
            /// <param name="fieldTransform">The RectTransform of the text field where the keyboard input will be directed.</param>
            /// <param name="canvas">The Canvas containing the text field, used to determine screen positions.</param>
            /// <returns>True if the virtual keyboard was successfully displayed, otherwise false.</returns>
            public static bool ShowVirtualKeyboard(EFloatingGamepadTextInputMode mode, RectTransform fieldTransform,
                Canvas canvas)
            {
                var rect = RectTransformUtility.PixelAdjustRect(fieldTransform, canvas);
                if (SteamUtils.ShowFloatingGamepadTextInput(mode, (int)rect.x, (int)rect.y, (int)rect.size.x, (int)rect.size.y))
                {
                    SteamTools.Events.InvokeOnKeyboardShown();
                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Displays a virtual keyboard over the game content, enabling direct input from the OS keyboard to the game.
            /// The keyboard's position is determined relative to the game's window to avoid obstructing the intended text field.
            /// </summary>
            /// <param name="mode">The desired input mode for the virtual keyboard, such as single-line or numeric input.</param>
            /// <param name="fieldPosition">The position of the text field in pixels, relative to the origin of the game window.</param>
            /// <param name="fieldSize">The size of the text field in pixels, specifying width and height.</param>
            /// <returns>A boolean value indicating whether the virtual keyboard was successfully shown.</returns>
            public static bool ShowVirtualKeyboard(EFloatingGamepadTextInputMode mode, float2 fieldPosition,
                float2 fieldSize)
            {
                if (SteamUtils.ShowFloatingGamepadTextInput(mode, Convert.ToInt32(fieldPosition.x),
                        Convert.ToInt32(fieldPosition.y), Convert.ToInt32(fieldSize.x), Convert.ToInt32(fieldSize.y)))
                {
                    SteamTools.Events.InvokeOnKeyboardShown();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public static string GetPingLocationString()
            {
                if (SteamNetworkingUtils.GetLocalPingLocation(out var location) < 0) 
                    return string.Empty;
                
                SteamNetworkingUtils.ConvertPingLocationToString(ref location, out var locationString, Steamworks.Constants.k_cchMaxSteamNetworkingPingLocationString);
                return locationString.Trim();
            }

            public static int PingLocation(string locationString)
            {
                if (SteamNetworkingUtils.GetLocalPingLocation(out var myLocation) < 0) 
                    return -1;

                SteamNetworkingUtils.ParsePingLocationString(locationString, out var remoteLocation);
                return SteamNetworkingUtils.EstimatePingTimeBetweenTwoLocations(ref myLocation, ref remoteLocation);
            }

            public static int PingBetweenLocations(string fromLocationString, string toLocationString)
            {
                SteamNetworkingUtils.ParsePingLocationString(fromLocationString, out var fromLocation);
                SteamNetworkingUtils.ParsePingLocationString(toLocationString, out var toLocation);
                return SteamNetworkingUtils.EstimatePingTimeBetweenTwoLocations(ref fromLocation, ref toLocation);
            }
        }

        public static class Server
        {
            public static string GetPingLocationString()
            {
                if (SteamGameServerNetworkingUtils.GetLocalPingLocation(out var location) < 0) 
                    return string.Empty;
                
                SteamGameServerNetworkingUtils.ConvertPingLocationToString(ref location, out var locationString, Steamworks.Constants.k_cchMaxSteamNetworkingPingLocationString);
                return locationString.Trim();
            }
            
            public static int PingLocation(string locationString)
            {
                if (SteamGameServerNetworkingUtils.GetLocalPingLocation(out var myLocation) < 0) 
                    return -1;

                SteamGameServerNetworkingUtils.ParsePingLocationString(locationString, out var remoteLocation);
                return SteamGameServerNetworkingUtils.EstimatePingTimeBetweenTwoLocations(ref myLocation, ref remoteLocation);
            }

            public static int PingBetweenLocations(string fromLocationString, string toLocationString)
            {
                SteamGameServerNetworkingUtils.ParsePingLocationString(fromLocationString, out var fromLocation);
                SteamGameServerNetworkingUtils.ParsePingLocationString(toLocationString, out var toLocation);
                return SteamGameServerNetworkingUtils.EstimatePingTimeBetweenTwoLocations(ref fromLocation, ref toLocation);
            }
        }
    }
}
#endif
