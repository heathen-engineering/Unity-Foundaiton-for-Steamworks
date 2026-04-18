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

namespace Heathen.SteamworksIntegration
{
    // =====================================================================
    // Settings provider — Project Settings > Steamworks
    // =====================================================================

    public class SteamToolsSettingsProvider : SettingsProvider
    {
        private SteamToolsSettings _settings;

        // Per-foldout toggle state (keyed by arbitrary string)
        private readonly Dictionary<string, bool> _foldouts = new();
        private string _newPlaytestName = string.Empty;

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

            // Ensure at least Main settings exist
            _settings.mainAppSettings ??= SteamToolsSettings.AppSettings.CreateDefault();

            // ---- App selector dropdown ----
            DrawAppSelector();

            EditorGUILayout.Space(6);

            // ---- Toolkit installed notice ----
            if (FoundationPackageDetector.IsToolkitInstalled)
            {
                EditorGUILayout.HelpBox(
                    "Toolkit for Steamworks is installed. Settings for Input, Inventory, " +
                    "and Server configuration are managed by the Toolkit settings provider " +
                    "(Project Settings > Player > Steamworks).",
                    MessageType.Info);
                EditorGUILayout.Space(4);
            }

            // ---- Action row ----
            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Knowledge Base"))
                Application.OpenURL("https://kb.heathen.group/steamworks");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            if (GUILayout.Button("Generate Code", GUILayout.Height(24)))
                GenerateCode();

            if (_settings.LastGenerated != default)
                EditorGUILayout.LabelField("Last generated: " + _settings.LastGenerated.ToString("yyyy-MM-dd HH:mm:ss"), EditorStyles.miniLabel);

            EditorGUILayout.Space(8);

            // ---- Main app ----
            DrawAppSection("Main", _settings.mainAppSettings, isRemovable: false);

            // ---- Demo app ----
            DrawDemoSection();

            // ---- Playtests ----
            DrawPlaytestsSection();
        }

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SteamToolsSettingsProvider("Project/Steamworks", SettingsScope.Project)
            {
                keywords = new HashSet<string>(new[]
                {
                    "Steamworks", "Steam", "App ID", "Achievement", "Stat", "Leaderboard"
                })
            };
        }

        // ----------------------------------------------------------------
        // Code generation — dispatches to Toolkit generator when available
        // ----------------------------------------------------------------

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

        // ----------------------------------------------------------------
        // App selector
        // ----------------------------------------------------------------

        private void DrawAppSelector()
        {
            var options = new List<string>();
            var indices = new List<int>();

            options.Add($"Main ({_settings.mainAppSettings.applicationId})");
            indices.Add(-1);

            if (_settings.demoAppSettings != null)
            {
                options.Add($"Demo ({_settings.demoAppSettings.applicationId})");
                indices.Add(-2);
            }

            for (var i = 0; i < _settings.playtestSettings.Count; i++)
            {
                var pt = _settings.playtestSettings[i];
                options.Add($"{pt.editorName} ({pt.applicationId})");
                indices.Add(i);
            }

            int currentDropdownIndex = indices.IndexOf(_settings.activeAppIndex);
            if (currentDropdownIndex < 0) currentDropdownIndex = 0;

            EditorGUI.indentLevel++;
            int newDropdownIndex = EditorGUILayout.Popup("Active Application", currentDropdownIndex, options.ToArray());
            EditorGUI.indentLevel--;

            int newAppIndex = indices[newDropdownIndex];
            if (newAppIndex != _settings.activeAppIndex)
            {
                Undo.RecordObject(_settings, "Change Active App");
                _settings.activeAppIndex = newAppIndex;
                EditorUtility.SetDirty(_settings);

                ApplyAppDefine();

                bool restart = EditorUtility.DisplayDialog(
                    "Restart Required",
                    "Changing the active App ID requires a domain reload. Restart Unity now?",
                    "Restart Now", "Later");
                if (restart)
                    EditorApplication.OpenProject(System.Environment.CurrentDirectory);
            }
        }

        private void ApplyAppDefine()
        {
            if (!_settings.ActiveApp.HasValue) return;

            var activeDefine = $"APP{_settings.ActiveApp.Value}";

            foreach (var target in new[] {
                UnityEditor.Build.NamedBuildTarget.Standalone,
                UnityEditor.Build.NamedBuildTarget.Server })
            {
                var raw    = PlayerSettings.GetScriptingDefineSymbols(target);
                var set    = new HashSet<string>(raw.Split(';', StringSplitOptions.RemoveEmptyEntries));

                // Remove stale APP#### defines
                var toRemove = new List<string>();
                foreach (var d in set)
                    if (d.StartsWith("APP") && d.Length > 3 && uint.TryParse(d.Substring(3), out _) && d != activeDefine)
                        toRemove.Add(d);

                foreach (var d in toRemove) set.Remove(d);
                set.Add(activeDefine);

                PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", set));
            }
        }

        // ----------------------------------------------------------------
        // Section helpers
        // ----------------------------------------------------------------

        private void DrawAppSection(string title, SteamToolsSettings.AppSettings app, bool isRemovable)
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 };
            EditorGUILayout.LabelField(" " + title, headerStyle);

            EditorGUI.indentLevel++;

            // App ID
            var idStr = EditorGUILayout.TextField("Application ID", app.applicationId.ToString());
            if (uint.TryParse(idStr, out var parsed) && parsed != app.applicationId)
            {
                Undo.RecordObject(_settings, "Change App ID");
                app.applicationId = parsed;
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            if (EditorGUILayout.LinkButton("Steamworks Partner Portal"))
                Application.OpenURL($"https://partner.steamgames.com/apps/landing/{app.applicationId}");

            EditorGUILayout.Space(4);

            DrawAchievementsList(app);
            DrawStatsList(app);
            DrawLeaderboardsList(app);

            EditorGUI.indentLevel--;
        }

        private void DrawDemoSection()
        {
            EditorGUILayout.Space(8);
            var headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 };
            EditorGUILayout.LabelField(" Demo", headerStyle);

            if (_settings.demoAppSettings != null)
            {
                EditorGUI.indentLevel++;
                if (EditorGUILayout.LinkButton("Remove Demo"))
                {
                    Undo.RecordObject(_settings, "Remove Demo");
                    _settings.demoAppSettings = null;
                    _settings.isDirty = true;
                    EditorUtility.SetDirty(_settings);
                    EditorGUI.indentLevel--;
                    return;
                }
                EditorGUI.indentLevel--;

                DrawAppSection("Demo", _settings.demoAppSettings, isRemovable: true);
            }
            else
            {
                EditorGUI.indentLevel++;
                if (GUILayout.Button("Add Demo App Settings"))
                {
                    Undo.RecordObject(_settings, "Add Demo");
                    _settings.demoAppSettings = new SteamToolsSettings.AppSettings { editorName = "Demo" };
                    _settings.isDirty = true;
                    EditorUtility.SetDirty(_settings);
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawPlaytestsSection()
        {
            EditorGUILayout.Space(8);
            var headerStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 16 };
            EditorGUILayout.LabelField(" Playtests", headerStyle);

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            _newPlaytestName = EditorGUILayout.TextField("Name", _newPlaytestName);
            if (GUILayout.Button("Add Playtest", GUILayout.Width(100)) && !string.IsNullOrWhiteSpace(_newPlaytestName))
            {
                Undo.RecordObject(_settings, "Add Playtest");
                _settings.playtestSettings.Add(new SteamToolsSettings.AppSettings
                {
                    editorName    = _newPlaytestName,
                    applicationId = 0
                });
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
                _newPlaytestName = string.Empty;
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;

            SteamToolsSettings.AppSettings toRemove = null;
            foreach (var pt in _settings.playtestSettings)
            {
                EditorGUILayout.Space(4);
                var headerStyle2 = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14 };
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(" " + pt.editorName, headerStyle2);
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                    toRemove = pt;
                EditorGUILayout.EndHorizontal();

                if (toRemove == null)
                    DrawAppSection(pt.editorName, pt, isRemovable: true);
            }

            if (toRemove != null)
            {
                Undo.RecordObject(_settings, "Remove Playtest");
                _settings.playtestSettings.Remove(toRemove);
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }
        }

        // ----------------------------------------------------------------
        // Achievements list
        // ----------------------------------------------------------------

        private void DrawAchievementsList(SteamToolsSettings.AppSettings app)
        {
            string key = app.editorName + "_ach";
            _foldouts.TryGetValue(key, out var open);
            open = EditorGUILayout.Foldout(open, $"Achievements ({app.achievements.Count})", true);
            _foldouts[key] = open;
            if (!open) return;

            EditorGUI.indentLevel++;

            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUI.contentColor = new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button("+ Add", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Undo.RecordObject(_settings, "Add Achievement");
                app.achievements.Add("NEW_ACHIEVEMENT");
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < app.achievements.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                var newName = EditorGUILayout.TextField(app.achievements[i]);
                if (newName != app.achievements[i])
                {
                    Undo.RecordObject(_settings, "Rename Achievement");
                    app.achievements[i] = newName;
                    _settings.isDirty = true;
                    EditorUtility.SetDirty(_settings);
                }
                GUI.contentColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    Undo.RecordObject(_settings, "Remove Achievement");
                    app.achievements.RemoveAt(i);
                    _settings.isDirty = true;
                    EditorUtility.SetDirty(_settings);
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        // ----------------------------------------------------------------
        // Stats list
        // ----------------------------------------------------------------

        private void DrawStatsList(SteamToolsSettings.AppSettings app)
        {
            string key = app.editorName + "_stats";
            _foldouts.TryGetValue(key, out var open);
            open = EditorGUILayout.Foldout(open, $"Stats ({app.stats.Count})", true);
            _foldouts[key] = open;
            if (!open) return;

            EditorGUI.indentLevel++;

            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUI.contentColor = new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button("+ Add", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Undo.RecordObject(_settings, "Add Stat");
                app.stats.Add("NewStat");
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < app.stats.Count; i++)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                var newName = EditorGUILayout.TextField(app.stats[i]);
                if (newName != app.stats[i])
                {
                    Undo.RecordObject(_settings, "Rename Stat");
                    app.stats[i] = newName;
                    _settings.isDirty = true;
                    EditorUtility.SetDirty(_settings);
                }
                GUI.contentColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                {
                    Undo.RecordObject(_settings, "Remove Stat");
                    app.stats.RemoveAt(i);
                    _settings.isDirty = true;
                    EditorUtility.SetDirty(_settings);
                    GUI.contentColor = color;
                    EditorGUILayout.EndHorizontal();
                    break;
                }
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        // ----------------------------------------------------------------
        // Leaderboards list
        // ----------------------------------------------------------------

        private void DrawLeaderboardsList(SteamToolsSettings.AppSettings app)
        {
            string key = app.editorName + "_lb";
            _foldouts.TryGetValue(key, out var open);
            open = EditorGUILayout.Foldout(open, $"Leaderboards ({app.leaderboards.Count})", true);
            _foldouts[key] = open;
            if (!open) return;

            EditorGUI.indentLevel++;

            var color = GUI.contentColor;
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            GUI.contentColor = new Color(0.4f, 1f, 0.4f);
            if (GUILayout.Button("+ Add", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                Undo.RecordObject(_settings, "Add Leaderboard");
                app.leaderboards.Add(new SteamToolsSettings.LeaderboardSetting
                {
                    name            = "New Leaderboard",
                    sortMethod      = ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending,
                    displayType     = ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric,
                    createIfNotFound = false
                });
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }
            GUI.contentColor = color;
            EditorGUILayout.EndHorizontal();

            SteamToolsSettings.LeaderboardSetting? lbToRemove = null;

            for (int i = 0; i < app.leaderboards.Count; i++)
            {
                var lb = app.leaderboards[i];

                string lbKey = app.editorName + "_lb_" + i;
                _foldouts.TryGetValue(lbKey, out var lbOpen);

                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                lbOpen = EditorGUILayout.Foldout(lbOpen, lb.name, true);
                _foldouts[lbKey] = lbOpen;

                GUI.contentColor = new Color(1f, 0.4f, 0.4f);
                if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.Width(25)))
                    lbToRemove = lb;
                GUI.contentColor = color;
                EditorGUILayout.EndHorizontal();

                if (lbOpen)
                {
                    EditorGUI.indentLevel++;

                    var newName = EditorGUILayout.TextField("Name", lb.name);
                    if (newName != lb.name)
                    {
                        Undo.RecordObject(_settings, "Rename Leaderboard");
                        lb.name          = newName;
                        _settings.isDirty = true;
                        EditorUtility.SetDirty(_settings);
                    }

                    var newSort = (ELeaderboardSortMethod)EditorGUILayout.EnumPopup("Sort Method", lb.sortMethod);
                    if (newSort != lb.sortMethod)
                    {
                        Undo.RecordObject(_settings, "Change Leaderboard Sort");
                        lb.sortMethod    = newSort;
                        _settings.isDirty = true;
                        EditorUtility.SetDirty(_settings);
                    }

                    var newDisplay = (ELeaderboardDisplayType)EditorGUILayout.EnumPopup("Display Type", lb.displayType);
                    if (newDisplay != lb.displayType)
                    {
                        Undo.RecordObject(_settings, "Change Leaderboard Display");
                        lb.displayType   = newDisplay;
                        _settings.isDirty = true;
                        EditorUtility.SetDirty(_settings);
                    }

                    var newCreate = EditorGUILayout.Toggle("Create If Not Found", lb.createIfNotFound);
                    if (newCreate != lb.createIfNotFound)
                    {
                        Undo.RecordObject(_settings, "Change Leaderboard Create");
                        lb.createIfNotFound = newCreate;
                        _settings.isDirty   = true;
                        EditorUtility.SetDirty(_settings);
                    }

                    EditorGUI.indentLevel--;
                }
            }

            if (lbToRemove != null)
            {
                Undo.RecordObject(_settings, "Remove Leaderboard");
                app.leaderboards.Remove(lbToRemove.Value);
                _settings.isDirty = true;
                EditorUtility.SetDirty(_settings);
            }

            EditorGUI.indentLevel--;
        }
    }
}
#endif
