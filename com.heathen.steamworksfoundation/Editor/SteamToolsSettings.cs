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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [Serializable]
    public class SteamToolsSettings
    {
        // Project-scoped, editor-only configuration. Lives in ProjectSettings/ so it is never
        // imported as an asset, never shipped in a build, and is not edited as project content.
        private const string SettingsPath = "ProjectSettings/SteamToolsSettings.json";
        // Previous location (inside Assets/). Older projects are migrated to SettingsPath on first load.
        private const string LegacySettingsPath = "Assets/Settings/SteamToolsSettings.json";
        private static SteamToolsSettings _instance;

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

#if HEATHEN_GAMEFRAMEWORK
        [Newtonsoft.Json.JsonIgnore]
#endif
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

        public string       LastGenerated     = "";
        public int          activeAppIndex    = -1;
#if HEATHEN_GAMEFRAMEWORK
        /// <summary>
        /// How the Steamworks subsystem comes up (Disabled / OnDemand / Automatic). Baked into the generated
        /// <c>SteamTools.Game</c> wrapper so the runtime subsystem honours it without reading any file.
        /// Defaults to <see cref="SubsystemStartMode.Automatic"/> — Steam initialises itself at startup with no
        /// component or code. Set <see cref="SubsystemStartMode.OnDemand"/> to instead init via the
        /// <c>Initialise Steam</c> component (or a manual <c>SteamTools.Interface.Initialise()</c> call), or
        /// <see cref="SubsystemStartMode.Disabled"/> to never init.
        /// </summary>
        public SubsystemStartMode startMode = SubsystemStartMode.Automatic;
#endif
        public AppSettings  mainAppSettings;
        public AppSettings  demoAppSettings;
        public List<string> dlcNames          = new();
        public List<uint>   dlc               = new();
        public SteamGameServerConfiguration defaultServerSettings;
        public List<AppSettings> playtestSettings = new();

        public List<NameAndID> inventoryItems = new();

        // Flattened unique lists rebuilt by CollectUniqueData().
        public List<uint>      appIds       = new();
        public List<string>    leaderboards = new();
        public List<string>    stats        = new();
        public List<string>    achievements = new();
        public List<string>    inputSets    = new();
        public List<string>    inputLayers  = new();
        public List<string>    inputActions = new();
        public List<NameAndID> items        = new();

        public AppSettings Get(uint appId)
        {
            if (mainAppSettings?.applicationId == appId) return mainAppSettings;
            if (demoAppSettings?.applicationId  == appId) return demoAppSettings;
            return playtestSettings.FirstOrDefault(p => p?.applicationId == appId);
        }

        public static SteamToolsSettings GetOrCreate()
        {
            if (_instance != null) return _instance;

            MigrateLegacySettings();

#if HEATHEN_GAMEFRAMEWORK
            // Ensure the Steam value-type converters are registered before the first load (InitializeOnLoad
            // order is not deterministic, and an editor hook can reach here first).
            SteamToolsSettingsConverters.EnsureRegistered();

            // Framework SettingsStore — same ProjectSettings/SteamToolsSettings.json path (resolved from the
            // type name), so Steamworks stores/loads identically to every other Heathen subsystem. Returns a
            // fresh default instance when no file exists yet (written on the first Save, the framework convention).
            _instance = Heathen.Editor.SettingsStore.Load<SteamToolsSettings>();
#else
            // Game Framework not yet installed — fall back to the legacy JsonUtility load at the same path.
            _instance = LoadFromDisk() ?? new SteamToolsSettings();
#endif
            _instance.mainAppSettings ??= AppSettings.CreateDefault();
            _instance.CollectUniqueData();
            return _instance;
        }

#if !HEATHEN_GAMEFRAMEWORK
        private static SteamToolsSettings LoadFromDisk()
        {
            if (!File.Exists(SettingsPath)) return null;
            try { return JsonUtility.FromJson<SteamToolsSettings>(File.ReadAllText(SettingsPath)); }
            catch { return null; }
        }
#endif

        /// <summary>
        /// Moves a pre-existing settings file from the legacy <c>Assets/Settings/</c> location to the
        /// project-scoped <c>ProjectSettings/</c> location so upgrading projects keep their configuration.
        /// The legacy <c>.meta</c> file (if present) is removed so Unity does not flag a missing asset.
        /// </summary>
        private static void MigrateLegacySettings()
        {
            if (File.Exists(SettingsPath) || !File.Exists(LegacySettingsPath))
                return;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath) ?? string.Empty);
                File.Copy(LegacySettingsPath, SettingsPath, overwrite: false);
                File.Delete(LegacySettingsPath);

                var legacyMeta = LegacySettingsPath + ".meta";
                if (File.Exists(legacyMeta))
                    File.Delete(legacyMeta);

                Debug.Log($"[Steamworks] Migrated Steam tools settings from '{LegacySettingsPath}' to '{SettingsPath}'.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Steamworks] Failed to migrate Steam tools settings from '{LegacySettingsPath}' to '{SettingsPath}': {e.Message}");
            }
        }

        public static void Save()
        {
            if (_instance == null) return;
            _instance.CollectUniqueData();
#if HEATHEN_GAMEFRAMEWORK
            Heathen.Editor.SettingsStore.Save(_instance);
#else
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath) ?? string.Empty);
            File.WriteAllText(SettingsPath, JsonUtility.ToJson(_instance, true));
#endif
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
