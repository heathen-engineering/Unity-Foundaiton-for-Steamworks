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
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using HeathenStyles = Heathen.Editor.HeathenEditorStyles;

namespace Heathen.SteamworksIntegration
{
    // =====================================================================
    // Settings provider — Project ▸ Subsystems ▸ Steamworks
    // Foundation subset of the Toolkit page: Achievements / Stats / Leaderboards only
    // (no DLC / Server / Input / Inventory / Import). Shares the same menu-bar + card visual language.
    // =====================================================================

    public class SteamToolsSettingsProvider : SettingsProvider
    {
        private SteamToolsSettings _settings;

        // Per-section expand state, keyed by an arbitrary string.
        private readonly Dictionary<string, bool> _foldouts = new();

        // Which app the body form is editing: -1 Main, -2 Demo, >=0 playtest index. Editor-view state only
        // (does NOT change the active build target — that is the menu-bar "Active" dropdown).
        private int _editIndex = -1;

        private const float TbBtn = 26f;

        // The Build/status button's state, polled at most a few times a second.
        private HeathenStyles.BuildStatus _buildStatus = HeathenStyles.BuildStatus.UpToDate;
        private double _lastStaleCheck;

        // True while inline-renaming the selected playtest (triggered from the top card's ⋮ menu).
        private bool _renamingApp;

        private bool Expanded(string key) { _foldouts.TryGetValue(key, out var v); return v; }
        private void SetExpanded(string key, bool v) => _foldouts[key] = v;

        public SteamToolsSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
            : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            _settings = SteamToolsSettings.GetOrCreate();
        }

        public override void OnGUI(string searchContext)
        {
            if (_settings == null)
            {
                _settings = SteamToolsSettings.GetOrCreate();
                return;
            }

            _settings.mainAppSettings ??= SteamToolsSettings.AppSettings.CreateDefault();

            DrawMenuBar();
            DrawAppForm();
        }

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            // When the Toolkit is installed it registers the richer Steamworks page at this same path; defer to
            // it so the two providers never collide and there is a single Project ▸ Subsystems ▸ Steamworks page.
            if (FoundationPackageDetector.IsToolkitInstalled)
                return null;

            return new SteamToolsSettingsProvider("Project/Subsystems/Steamworks", SettingsScope.Project)
            {
                keywords = new HashSet<string>(new[]
                {
                    "Steamworks", "Steam", "App ID", "Achievement", "Stat", "Leaderboard"
                })
            };
        }

        // ── Code generation (dispatches to the Toolkit generator if it is ever present) ─────────────
        private void GenerateCode()
        {
            var generate = FindToolkitGenerator();
            if (generate != null)
                generate.Invoke(null, new object[] { _settings });
            else
                SteamToolsCodeGenerator.Generate(_settings);
        }

        private static System.Reflection.MethodInfo FindToolkitGenerator()
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (asm.GetName().Name != "Heathen.Steamworks.Editor") continue;
                var type = asm.GetType("Heathen.SteamworksIntegration.ToolkitSteamToolsCodeGenerator");
                if (type != null)
                    return type.GetMethod("Generate", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            }
            return null;
        }

        // ── Menu bar ─────────────────────────────────────────────────────────────
        private void DrawMenuBar()
        {
            var options = new List<string>();
            var indices = new List<int>();
            options.Add($"Main ({_settings.mainAppSettings.applicationId})"); indices.Add(-1);
            if (_settings.demoAppSettings != null)
            {
                options.Add($"Demo ({_settings.demoAppSettings.applicationId})"); indices.Add(-2);
            }
            for (var i = 0; i < _settings.playtestSettings.Count; i++)
            {
                var pt = _settings.playtestSettings[i];
                options.Add($"{pt.editorName} ({pt.applicationId})"); indices.Add(i);
            }

            int cur = indices.IndexOf(_settings.activeAppIndex);
            if (cur < 0) cur = 0;

            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                DrawBuildStatusButton();

                int sel = EditorGUILayout.Popup(cur, options.ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(180));
                if (sel != cur && indices[sel] != _settings.activeAppIndex)
                {
                    _settings.activeAppIndex = indices[sel];
                    SteamToolsSettings.Save();
                    ApplyAppDefine();
                    if (EditorUtility.DisplayDialog(
                            "Restart Required",
                            "Changing the active App ID requires a domain reload. Restart Unity now?",
                            "Restart Now", "Later"))
                        EditorApplication.OpenProject(System.Environment.CurrentDirectory);
                }
                else
                {
                    ApplyAppDefine();
                }

                if (GUILayout.Button(new GUIContent("Portal", "Open the active app's Steamworks partner page"),
                    EditorStyles.toolbarButton, GUILayout.Width(52)))
                    Application.OpenURL("https://partner.steamgames.com/apps/landing/" + (_settings.ActiveApp ?? _settings.mainAppSettings.applicationId));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(ToolbarIcon("_Help", "Help, links & tools", "Help"),
                    EditorStyles.toolbarButton, GUILayout.Width(TbBtn)))
                    ShowHelpMenu();
            }
        }

        // Left-most menu-bar button; colour + label reflect the generated-wrapper state (Ready/Build/Update/Error).
        private void DrawBuildStatusButton()
        {
            if (EditorApplication.timeSinceStartup - _lastStaleCheck > 0.5)
            {
                try
                {
                    _buildStatus = !SteamToolsCodeGenerator.ScriptExists() ? HeathenStyles.BuildStatus.NotBuilt
                                 : SteamToolsCodeGenerator.IsStale(_settings) ? HeathenStyles.BuildStatus.Dirty
                                 : HeathenStyles.BuildStatus.UpToDate;
                }
                catch { _buildStatus = HeathenStyles.BuildStatus.Error; }
                _lastStaleCheck = EditorApplication.timeSinceStartup;
            }

            if (HeathenStyles.BuildStatusButton(_buildStatus))
            {
                GenerateCode();
                _lastStaleCheck = 0;
            }
        }

        private static GUIContent ToolbarIcon(string iconName, string tooltip, string fallbackText)
        {
            var c = EditorGUIUtility.IconContent(iconName);
            return (c != null && c.image != null)
                ? new GUIContent(c.image, tooltip)
                : new GUIContent(fallbackText, tooltip);
        }

        private static GUIContent AppMenuIcon()
        {
            var c = EditorGUIUtility.IconContent("_Menu");
            return (c != null && c.image != null) ? new GUIContent(c.image, "More options") : new GUIContent("⋮", "More options");
        }

        private void ShowAppContextMenu(SteamToolsSettings.AppSettings app)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Steamworks Portal"), false,
                () => Application.OpenURL($"https://partner.steamgames.com/apps/landing/{app.applicationId}"));
            menu.AddItem(new GUIContent("Set Spacewars Values"), false,
                () => { GUI.FocusControl(null); app.SetDefault(); SteamToolsSettings.Save(); });
            if (_editIndex >= 0)
            {
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("Rename…"), false, () => _renamingApp = true);
                menu.AddItem(new GUIContent("Remove Playtest"), false, () =>
                {
                    GUI.FocusControl(null);
                    _settings.playtestSettings.RemoveAt(_editIndex);
                    SteamToolsSettings.Save();
                    _editIndex = -1;
                    _renamingApp = false;
                });
            }
            menu.ShowAsContext();
        }

        private void ShowHelpMenu()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Generate Code Now"), false, () => { GenerateCode(); _lastStaleCheck = 0; });
#if HEATHEN_GAMEFRAMEWORK
            menu.AddItem(new GUIContent("Subsystem Start Mode…"), false, () => SettingsService.OpenProjectSettings("Project/Subsystems"));
#endif
            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Knowledge Base"), false, () => Application.OpenURL("https://kb.heathen.group/steamworks"));
            menu.AddItem(new GUIContent("Support (Discord)"), false, () => Application.OpenURL("https://discord.gg/heathen-group-463483739612381204"));
            menu.ShowAsContext();
        }

        private void ApplyAppDefine()
        {
            if (!_settings.ActiveApp.HasValue) return;

            var activeDefine = $"APP{_settings.ActiveApp.Value}";

            foreach (var target in new[] {
                UnityEditor.Build.NamedBuildTarget.Standalone,
                UnityEditor.Build.NamedBuildTarget.Server })
            {
                var raw = PlayerSettings.GetScriptingDefineSymbols(target);
                var set = new HashSet<string>(raw.Split(';', StringSplitOptions.RemoveEmptyEntries));

                var toRemove = new List<string>();
                foreach (var d in set)
                    if (d.StartsWith("APP") && d.Length > 3 && uint.TryParse(d.Substring(3), out _) && d != activeDefine)
                        toRemove.Add(d);

                foreach (var d in toRemove) set.Remove(d);
                set.Add(activeDefine);

                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", set));
            }
        }

        // ── App-selector form (one-row top card + sections) ─────────────────────────
        private void DrawAppForm()
        {
            var labels = new List<string>();
            var keys   = new List<int>();
            labels.Add($"Main ({_settings.mainAppSettings.applicationId})"); keys.Add(-1);
            labels.Add(_settings.demoAppSettings != null
                ? $"Demo ({_settings.demoAppSettings.applicationId})"
                : "Demo (create)");
            keys.Add(-2);
            for (int i = 0; i < _settings.playtestSettings.Count; i++)
            {
                var p = _settings.playtestSettings[i];
                labels.Add($"{p.editorName} ({p.applicationId})");
                keys.Add(i);
            }
            const int NewPlaytestKey = int.MinValue;
            labels.Add("＋ New Playtest…"); keys.Add(NewPlaytestKey);

            int cur = keys.IndexOf(_editIndex);
            if (cur < 0) { _editIndex = -1; cur = 0; }

            EditorGUILayout.Space(4);
            EditorGUIUtility.labelWidth = 90f;

            SteamToolsSettings.AppSettings app = null;
            using (HeathenStyles.BeginCard())
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("Editing", GUILayout.Width(48));

                int sel = EditorGUILayout.Popup(cur, labels.ToArray(), GUILayout.Width(150));
                if (sel != cur)
                {
                    int key = keys[sel];
                    GUI.FocusControl(null);
                    _renamingApp = false;
                    if (key == NewPlaytestKey)
                    {
                        _settings.playtestSettings.Add(new SteamToolsSettings.AppSettings
                            { editorName = NextPlaytestName(), applicationId = 0 });
                        SteamToolsSettings.Save();
                        _editIndex = _settings.playtestSettings.Count - 1;
                    }
                    else if (key == -2 && _settings.demoAppSettings == null)
                    {
                        _settings.demoAppSettings = new SteamToolsSettings.AppSettings { editorName = "Demo" };
                        SteamToolsSettings.Save();
                        _editIndex = -2;
                    }
                    else
                    {
                        _editIndex = key;
                    }
                }

                app = ResolveEditApp();
                if (app != null)
                {
                    var idStr = EditorGUILayout.TextField(app.applicationId.ToString(), GUILayout.Width(90));
                    if (uint.TryParse(idStr, out var buffer) && buffer != app.applicationId)
                    {
                        app.applicationId = buffer;
                        SteamToolsSettings.Save();
                    }

                    if (GUILayout.Button(new GUIContent("Clear", "Clear all achievements, stats and leaderboards for this app."), GUILayout.Width(52)))
                    {
                        GUI.FocusControl(null);
                        app.Clear();
                        SteamToolsSettings.Save();
                    }

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(AppMenuIcon(), EditorStyles.toolbarButton, GUILayout.Width(24)))
                        ShowAppContextMenu(app);
                }
            }

            if (app == null) return;

            if (_renamingApp && _editIndex >= 0)
            {
                using (HeathenStyles.BeginCard())
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("Name", GUILayout.Width(48));
                    var newName = EditorGUILayout.TextField(app.editorName);
                    if (newName != app.editorName) { app.editorName = newName; SteamToolsSettings.Save(); }
                    if (GUILayout.Button("Done", GUILayout.Width(52))) { GUI.FocusControl(null); _renamingApp = false; }
                }
            }

            DrawAchievementsList(app);
            DrawStatsList(app);
            DrawLeaderboardsList(app);
        }

        private SteamToolsSettings.AppSettings ResolveEditApp()
        {
            if (_editIndex == -1) return _settings.mainAppSettings;
            if (_editIndex == -2) return _settings.demoAppSettings;
            if (_editIndex >= 0 && _editIndex < _settings.playtestSettings.Count) return _settings.playtestSettings[_editIndex];
            return _settings.mainAppSettings;
        }

        private string NextPlaytestName() => $"Playtest {_settings.playtestSettings.Count + 1}";

        // ── Leaderboard friendly Sort/Type mapping ──────────────────────────────
        private static readonly string[] LbSortLabels = { "Ascending", "Descending" };
        private static readonly string[] LbTypeLabels = { "Numeric", "Seconds", "Mili-seconds" };

        private static int SortToIdx(ELeaderboardSortMethod m) =>
            m == ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending ? 1 : 0;
        private static ELeaderboardSortMethod IdxToSort(int i) =>
            i == 1 ? ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending
                   : ELeaderboardSortMethod.k_ELeaderboardSortMethodAscending;

        private static int TypeToIdx(ELeaderboardDisplayType t) => t switch
        {
            ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeSeconds      => 1,
            ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds => 2,
            _                                                                 => 0,
        };
        private static ELeaderboardDisplayType IdxToType(int i) => i switch
        {
            1 => ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeSeconds,
            2 => ELeaderboardDisplayType.k_ELeaderboardDisplayTypeTimeMilliSeconds,
            _ => ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric,
        };

        // ── Sections (cards) ────────────────────────────────────────────────────
        private void DrawAchievementsList(SteamToolsSettings.AppSettings app)
        {
            string key = app.editorName + "_ach";
            using (HeathenStyles.BeginCard())
            {
                SetExpanded(key, HeathenStyles.CardHeader(Expanded(key), "Achievements", app.achievements.Count,
                    () => { GUI.FocusControl(null); app.achievements.Add("NEW_ACHIEVEMENT"); SteamToolsSettings.Save(); }, "Add an achievement"));
                if (!Expanded(key)) return;

                using (HeathenStyles.Indent())
                for (int i = 0; i < app.achievements.Count; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var newName = EditorGUILayout.TextField(app.achievements[i]);
                        if (newName != app.achievements[i]) { app.achievements[i] = newName; SteamToolsSettings.Save(); }
                        if (HeathenStyles.RemoveButton())
                        {
                            GUI.FocusControl(null);
                            app.achievements.RemoveAt(i);
                            SteamToolsSettings.Save();
                            break;
                        }
                    }
                }
            }
        }

        private void DrawStatsList(SteamToolsSettings.AppSettings app)
        {
            string key = app.editorName + "_stats";
            using (HeathenStyles.BeginCard())
            {
                SetExpanded(key, HeathenStyles.CardHeader(Expanded(key), "Stats", app.stats.Count,
                    () => { GUI.FocusControl(null); app.stats.Add("NewStat"); SteamToolsSettings.Save(); }, "Add a stat"));
                if (!Expanded(key)) return;

                using (HeathenStyles.Indent())
                for (int i = 0; i < app.stats.Count; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var newName = EditorGUILayout.TextField(app.stats[i]);
                        if (newName != app.stats[i]) { app.stats[i] = newName; SteamToolsSettings.Save(); }
                        if (HeathenStyles.RemoveButton())
                        {
                            GUI.FocusControl(null);
                            app.stats.RemoveAt(i);
                            SteamToolsSettings.Save();
                            break;
                        }
                    }
                }
            }
        }

        private void DrawLeaderboardsList(SteamToolsSettings.AppSettings app)
        {
            string key = app.editorName + "_lb";
            using (HeathenStyles.BeginCard())
            {
                SetExpanded(key, HeathenStyles.CardHeader(Expanded(key), "Leaderboards", app.leaderboards.Count,
                    () =>
                    {
                        GUI.FocusControl(null);
                        app.leaderboards.Add(new SteamToolsSettings.LeaderboardSetting
                        {
                            name             = "New Leaderboard",
                            sortMethod       = ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending,
                            displayType      = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric,
                            createIfNotFound = false
                        });
                        SteamToolsSettings.Save();
                    }, "Add a leaderboard"));
                if (!Expanded(key)) return;

                using (HeathenStyles.Indent())
                for (int i = 0; i < app.leaderboards.Count; i++)
                {
                    var lb = app.leaderboards[i];
                    bool newCreate; string newName; bool remove;
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        newCreate = GUILayout.Toggle(lb.createIfNotFound,
                            new GUIContent(string.Empty, "Create this leaderboard on Steam if it does not already exist. When on, choose a Sort and Type below."),
                            GUILayout.Width(18));
                        newName = EditorGUILayout.TextField(lb.name);
                        remove = HeathenStyles.RemoveButton();
                    }

                    if (remove)
                    {
                        app.leaderboards.RemoveAt(i);
                        SteamToolsSettings.Save();
                        break;
                    }

                    var newSort    = lb.sortMethod;
                    var newDisplay = lb.displayType;
                    if (newCreate)
                    {
                        EditorGUI.indentLevel++;
                        newSort    = IdxToSort(EditorGUILayout.Popup("Sort", SortToIdx(newSort),    LbSortLabels));
                        newDisplay = IdxToType(EditorGUILayout.Popup("Type", TypeToIdx(newDisplay), LbTypeLabels));
                        EditorGUI.indentLevel--;
                    }

                    if (newName != lb.name || newSort != lb.sortMethod || newDisplay != lb.displayType || newCreate != lb.createIfNotFound)
                    {
                        lb.name             = newName;
                        lb.sortMethod       = newSort;
                        lb.displayType      = newDisplay;
                        lb.createIfNotFound = newCreate;
                        app.leaderboards[i] = lb;
                        SteamToolsSettings.Save();
                    }
                }
            }
        }
    }
}
#endif
