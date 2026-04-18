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

#if UNITY_EDITOR && !DISABLESTEAMWORKS && STEAM_INSTALLED
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    public class SteamToolsSettings : ScriptableObject
    {
        [Serializable]
        public struct NameAndID : IEquatable<NameAndID>, IComparable<NameAndID>
        {
            public string name;
            public int id;

            public int CompareTo(NameAndID other) => string.Compare(name, other.name, StringComparison.Ordinal);
            public bool Equals(NameAndID other) => string.Equals(name, other.name);
            public override bool Equals(object obj) => obj is NameAndID other && Equals(other);
            public override int GetHashCode() => name != null ? name.GetHashCode() : 0;
        }

        [Serializable]
        public struct LeaderboardSetting : IEquatable<LeaderboardSetting>
        {
            public string name;
            public ELeaderboardSortMethod sortMethod;
            public ELeaderboardDisplayType displayType;
            public bool createIfNotFound;

            public bool Equals(LeaderboardSetting other) =>
                name == other.name &&
                sortMethod == other.sortMethod &&
                displayType == other.displayType &&
                createIfNotFound == other.createIfNotFound;

            public override bool Equals(object obj) => obj is LeaderboardSetting other && Equals(other);
            public override int GetHashCode() => HashCode.Combine(name, sortMethod, displayType, createIfNotFound);
        }

        [Serializable]
        public class AppSettings
        {
            public string editorName;
            public uint applicationId;
            public List<LeaderboardSetting> leaderboards   = new();
            public List<StatData>           stats          = new();
            public List<AchievementData>    achievements   = new();
            public List<string>             actionSets      = new();
            public List<string>             actionSetLayers = new();
            public List<InputActionData>    actions         = new();

            public static AppSettings CreateDefault()
            {
                var app = new AppSettings
                {
                    editorName    = "Main",
                    applicationId = 480
                };
                app.leaderboards.Add(new LeaderboardSetting
                {
                    name            = "Feet Traveled",
                    sortMethod      = ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending,
                    displayType     = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric,
                    createIfNotFound = false
                });
                app.stats.Add("AverageSpeed");
                app.stats.Add("FeetTraveled");
                app.stats.Add("MaxFeetTraveled");
                app.stats.Add("NumGames");
                app.stats.Add("NumLosses");
                app.stats.Add("NumWins");
                app.stats.Add("Unused2");
                app.achievements.Add("ACH_TRAVEL_FAR_ACCUM");
                app.achievements.Add("ACH_TRAVEL_FAR_SINGLE");
                app.achievements.Add("ACH_WIN_100_GAMES");
                app.achievements.Add("ACH_WIN_ONE_GAME");
                app.achievements.Add("NEW_ACHIEVEMENT_0_4");
                app.actionSets.Add("menu_controls");
                app.actionSets.Add("ship_controls");
                app.actionSetLayers.Add("thrust_action_layer");
                app.actions.Add(new("analog_controls",  InputActionType.Analog));
                app.actions.Add(new("backward_thrust",  InputActionType.Digital));
                app.actions.Add(new("fire_lasers",      InputActionType.Digital));
                app.actions.Add(new("forward_thrust",   InputActionType.Digital));
                app.actions.Add(new("menu_cancel",      InputActionType.Digital));
                app.actions.Add(new("menu_down",        InputActionType.Digital));
                app.actions.Add(new("menu_left",        InputActionType.Digital));
                app.actions.Add(new("menu_right",       InputActionType.Digital));
                app.actions.Add(new("menu_select",      InputActionType.Digital));
                app.actions.Add(new("menu_up",          InputActionType.Digital));
                app.actions.Add(new("pause_menu",       InputActionType.Digital));
                app.actions.Add(new("turn_left",        InputActionType.Digital));
                app.actions.Add(new("turn_right",       InputActionType.Digital));
                return app;
            }

            public static AppSettings CreateEmpty() => new() { editorName = "Empty" };

            public void Clear()
            {
                leaderboards.Clear();
                stats.Clear();
                achievements.Clear();
                actionSets.Clear();
                actionSetLayers.Clear();
                actions.Clear();
            }

            public void SetDefault()
            {
                Clear();
                var def = CreateDefault();
                applicationId = def.applicationId;
                leaderboards.AddRange(def.leaderboards);
                stats.AddRange(def.stats);
                achievements.AddRange(def.achievements);
                actionSets.AddRange(def.actionSets);
                actionSetLayers.AddRange(def.actionSetLayers);
                actions.AddRange(def.actions);
            }
        }

        public uint? ActiveApp
        {
            get
            {
                if (activeAppIndex == -1) return mainAppSettings?.applicationId;
                if (activeAppIndex == -2) return demoAppSettings?.applicationId;
                if (activeAppIndex >= 0 && activeAppIndex < playtestSettings.Count)
                    return playtestSettings[activeAppIndex]?.applicationId;
                return null;
            }
        }

        public DateTime LastGenerated;
        [HideInInspector] public int          activeAppIndex    = -1;
        [HideInInspector] public AppSettings  mainAppSettings   = AppSettings.CreateDefault();
        [HideInInspector] public AppSettings  demoAppSettings;
        [HideInInspector] public List<string> dlcNames          = new();
        [HideInInspector] public List<uint>   dlc               = new();
        [HideInInspector] public SteamGameServerConfiguration defaultServerSettings;
        [HideInInspector] public List<AppSettings> playtestSettings = new();

        // Inventory items — entered directly (name + definition id) since
        // InventorySettings runtime management lives in Toolkit.
        [HideInInspector] public List<NameAndID> inventoryItems = new();

        // Flattened unique lists rebuilt by CollectUniqueData().
        [HideInInspector] public List<uint>      appIds       = new();
        [HideInInspector] public List<string>    leaderboards = new();
        [HideInInspector] public List<string>    stats        = new();
        [HideInInspector] public List<string>    achievements = new();
        [HideInInspector] public List<string>    inputSets    = new();
        [HideInInspector] public List<string>    inputLayers  = new();
        [HideInInspector] public List<string>    inputActions = new();
        [HideInInspector] public List<NameAndID> items        = new();
        [HideInInspector] public bool            isDirty      = true;

        public AppSettings Get(uint appId)
        {
            if (mainAppSettings?.applicationId == appId) return mainAppSettings;
            if (demoAppSettings?.applicationId  == appId) return demoAppSettings;
            return playtestSettings.FirstOrDefault(p => p?.applicationId == appId);
        }

        public static SteamToolsSettings GetOrCreate()
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:SteamToolsSettings");
            if (guids != null && guids.Length > 0)
            {
                var path  = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                var found = UnityEditor.AssetDatabase.LoadAssetAtPath<SteamToolsSettings>(path);
                if (found) return found;
            }

            const string defaultPath = "Assets/Settings/SteamToolsSettings.asset";
            System.IO.Directory.CreateDirectory(
                System.IO.Path.GetDirectoryName(defaultPath) ?? string.Empty);

            var asset = CreateInstance<SteamToolsSettings>();
            UnityEditor.AssetDatabase.CreateAsset(asset, defaultPath);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log("[SteamToolsSettings] Created new settings asset at " + defaultPath);
            return asset;
        }

        public void CollectUniqueData()
        {
            appIds.Clear();
            leaderboards.Clear();
            stats.Clear();
            achievements.Clear();
            inputSets.Clear();
            inputLayers.Clear();
            inputActions.Clear();
            items.Clear();

            var uniqueAppIds       = new HashSet<uint>();
            var uniqueLeaderboards = new HashSet<string>();
            var uniqueStats        = new HashSet<string>();
            var uniqueAchievements = new HashSet<string>();
            var uniqueInputSets    = new HashSet<string>();
            var uniqueInputLayers  = new HashSet<string>();
            var uniqueInputActions = new HashSet<string>();
            var uniqueItems        = new HashSet<NameAndID>();

            foreach (var item in inventoryItems.Where(i => !string.IsNullOrEmpty(i.name)))
                uniqueItems.Add(item);

            void CollectFromApp(AppSettings app)
            {
                if (app == null) return;
                uniqueAppIds.Add(app.applicationId);
                foreach (var lb  in app.leaderboards.Where(lb  => !string.IsNullOrEmpty(lb.name)))       uniqueLeaderboards.Add(lb.name);
                foreach (var s   in app.stats.Where(s         => !string.IsNullOrEmpty(s)))              uniqueStats.Add(s);
                foreach (var a   in app.achievements.Where(a  => !string.IsNullOrEmpty(a)))              uniqueAchievements.Add(a);
                foreach (var set in app.actionSets.Where(set  => !string.IsNullOrEmpty(set)))            uniqueInputSets.Add(set);
                foreach (var lay in app.actionSetLayers.Where(lay => !string.IsNullOrEmpty(lay)))        uniqueInputLayers.Add(lay);
                foreach (var act in app.actions.Where(act     => !string.IsNullOrEmpty(act.Name)))       uniqueInputActions.Add(act.Name);
            }

            CollectFromApp(mainAppSettings);
            CollectFromApp(demoAppSettings);
            foreach (var pt in playtestSettings) CollectFromApp(pt);

            appIds.AddRange(uniqueAppIds);
            leaderboards.AddRange(uniqueLeaderboards);
            stats.AddRange(uniqueStats);
            achievements.AddRange(uniqueAchievements);
            inputSets.AddRange(uniqueInputSets);
            inputLayers.AddRange(uniqueInputLayers);
            inputActions.AddRange(uniqueInputActions);
            items.AddRange(uniqueItems);

            leaderboards.Sort();
            stats.Sort();
            achievements.Sort();
            inputSets.Sort();
            inputLayers.Sort();
            inputActions.Sort();
            dlcNames.Sort();
            items.Sort();

            isDirty = true;
        }

        public static string MakeValidIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "_unknown";

            var identifier = Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");
            identifier = Regex.Replace(identifier, @"_+", "_");
            identifier = identifier.Trim('_');

            if (string.IsNullOrEmpty(identifier))
            {
                var hexPart = string.Join("", name.Select(c => ((int)c).ToString("X2")));
                return $"_Hex_{hexPart}";
            }

            if (char.IsDigit(identifier[0]))
                identifier = "_" + identifier;

            if (IsReservedKeyword(identifier))
                identifier = "@" + identifier;

            return identifier;
        }

        public static bool IsReservedKeyword(string word)
        {
            var keywords = new HashSet<string> {
                "abstract","as","base","bool","break","byte","case","catch","char","checked",
                "class","const","continue","decimal","default","delegate","do","double","else",
                "enum","event","explicit","extern","false","finally","fixed","float","for",
                "foreach","goto","if","implicit","in","int","interface","internal","is","lock",
                "long","namespace","new","null","object","operator","out","override","params",
                "private","protected","public","readonly","ref","return","sbyte","sealed",
                "short","sizeof","stackalloc","static","string","struct","switch","this","throw",
                "true","try","typeof","uint","ulong","unchecked","unsafe","ushort","using",
                "virtual","void","volatile","while"
            };
            return keywords.Contains(word);
        }
    }
}
#endif
