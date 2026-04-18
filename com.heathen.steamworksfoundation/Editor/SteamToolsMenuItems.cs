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

#if UNITY_EDITOR && !HEATHEN_TOOLKIT_INSTALLED
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public static class SteamToolsMenuItems
    {
        // The version of Steamworks.NET that Foundation is validated against.
        private const string SteamworksNetVersion163 =
            "https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net#2025.163.0";
        private const string SteamworksNetVersion162 =
            "https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net#2025.162.1";
        private const string SteamworksNetVersion161 =
            "https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net#2025.161.0";
        private const string SteamworksNetLatest =
            "https://github.com/rlabrecque/Steamworks.NET.git?path=/com.rlabrecque.steamworks.net";

        // ----------------------------------------------------------------
        // Menu items
        // ----------------------------------------------------------------

        [MenuItem("Help/Heathen/Steamworks Foundation/Install Steamworks.NET v1.63.0 (Recommended)", priority = 1)]
        public static void InstallSteamworks163() => StartCoroutine(DoInstall(SteamworksNetVersion163));

        [MenuItem("Help/Heathen/Steamworks Foundation/Install Steamworks.NET v1.62.1", priority = 2)]
        public static void InstallSteamworks162() => StartCoroutine(DoInstall(SteamworksNetVersion162));

        [MenuItem("Help/Heathen/Steamworks Foundation/Install Steamworks.NET v1.61.0", priority = 3)]
        public static void InstallSteamworks161() => StartCoroutine(DoInstall(SteamworksNetVersion161));

        [MenuItem("Help/Heathen/Steamworks Foundation/Install Steamworks.NET (Latest — Not Tested)", priority = 4)]
        public static void InstallSteamworksLatest() => StartCoroutine(DoInstall(SteamworksNetLatest));

        [MenuItem("Help/Heathen/Steamworks Foundation/Open Settings", priority = 10)]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/Steamworks");

        [MenuItem("Help/Heathen/Steamworks Foundation/Documentation", priority = 11)]
        public static void Documentation() => Application.OpenURL("https://kb.heathen.group/steamworks");

        [MenuItem("Help/Heathen/Steamworks Foundation/Support", priority = 12)]
        public static void Support() => Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");

        // ----------------------------------------------------------------
        // Domain-reload check: prompt to install Steamworks.NET if absent
        // ----------------------------------------------------------------

        [InitializeOnLoadMethod]
        public static void CheckForSteamworksInstall()
        {
            StartCoroutine(ValidateInstall());
        }

        private static IEnumerator ValidateInstall()
        {
            var listReq = Client.List(offlineMode: false);
            while (!listReq.IsCompleted) yield return null;

            if (listReq.Status != StatusCode.Success)
            {
                Debug.LogError("[SteamToolsMenuItems] Failed to query Package Manager: " + listReq.Error?.message);
                yield break;
            }

#if !STEAM_INSTALLED
            bool install = EditorUtility.DisplayDialog(
                "Heathen — Steamworks Foundation",
                "Steamworks.NET is required but was not found. " +
                "Would you like to install the recommended version (v1.63.0) now?",
                "Install", "Cancel");

            if (install)
                yield return DoInstall(SteamworksNetVersion163);
#endif
        }

        // ----------------------------------------------------------------
        // Package install helper
        // ----------------------------------------------------------------

        private static IEnumerator DoInstall(string uri)
        {
            if (SessionState.GetBool("SteamworksInstalling", false))
            {
                Debug.LogWarning("[SteamToolsMenuItems] An install is already in progress.");
                yield break;
            }

            SessionState.SetBool("SteamworksInstalling", true);

            AddRequest req = Client.Add(uri);
            Debug.Log($"[SteamToolsMenuItems] Installing Steamworks.NET from {uri} …");

            while (!req.IsCompleted) yield return null;

            if (req.Status == StatusCode.Success)
                Debug.Log($"[SteamToolsMenuItems] Steamworks.NET {req.Result.version} installed successfully.");
            else
                Debug.LogError($"[SteamToolsMenuItems] Steamworks.NET install failed: {req.Error?.message}");

            SessionState.SetBool("SteamworksInstalling", false);
        }

        // ----------------------------------------------------------------
        // Minimal editor coroutine runner (no MonoBehaviour required)
        // ----------------------------------------------------------------

        private static List<IEnumerator> _coroutines;

        private static void StartCoroutine(IEnumerator routine)
        {
            if (_coroutines == null)
            {
                EditorApplication.update -= Tick;
                EditorApplication.update += Tick;
                _coroutines = new List<IEnumerator>();
            }
            _coroutines.Add(routine);
        }

        private static void Tick()
        {
            if (_coroutines == null || _coroutines.Count == 0) return;

            var done = new List<IEnumerator>();
            foreach (var c in _coroutines)
                if (!c.MoveNext()) done.Add(c);
            foreach (var d in done) _coroutines.Remove(d);
        }
    }
}
#endif
