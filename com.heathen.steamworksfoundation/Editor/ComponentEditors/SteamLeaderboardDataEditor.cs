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
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [CustomEditor(typeof(SteamLeaderboardData), true)]
    public class SteamLeaderboardDataEditor : ModularEditor
    {
        private SteamToolsSettings _settings;

        private string[] _options;
        private int _selectedIndex;
        private SerializedProperty _apiNameProp;
        private SerializedProperty _createIfMissingProp;
        private SerializedProperty _createAsDisplayProp;
        private SerializedProperty _createWithSortProp;

        protected override Type[] AllowedTypes => new Type[]
        {
            // SteamLeaderboardDataEvents has a different (UGC-extended) version in Toolkit;
            // both are named identically so Foundation's is guarded when Toolkit is installed.
            // The Toolkit's version is still discovered at runtime via ModularEventsAttribute
            // reflection — it just won't appear in the add-field dropdown when Toolkit is active.
#if !HEATHEN_TOOLKIT_INSTALLED
            typeof(SteamLeaderboardDataEvents),
#endif
            typeof(SteamLeaderboardDisplay),
            typeof(SteamLeaderboardName),
            typeof(SteamLeaderboardRank),
            typeof(SteamLeaderboardUpload),
            typeof(SteamLeaderboardUserEntry),
        };

        private void OnEnable()
        {
            _apiNameProp        = serializedObject.FindProperty("apiName");
            _createIfMissingProp = serializedObject.FindProperty("createIfMissing");
            _createAsDisplayProp = serializedObject.FindProperty("createAsDisplay");
            _createWithSortProp  = serializedObject.FindProperty("createWithSort");

            _settings = SteamToolsSettings.GetOrCreate();
            RefreshOptions();
        }

        private void RefreshOptions()
        {
            var settings = SteamToolsSettings.GetOrCreate();
            var list = settings != null && settings.leaderboards != null
                ? settings.leaderboards
                : new List<string>();

            var temp = new string[list.Count + 1];
            for (int i = 0; i < list.Count; i++)
                temp[i] = list[i];
            temp[list.Count] = "<new>";

            _options = temp;

            var current = _apiNameProp.stringValue;
            _selectedIndex = Array.IndexOf(_options, current);
            if (_selectedIndex < 0)
                _selectedIndex = _options.Length - 1; // default to <new>
        }

        public override void OnInspectorGUI()
        {
            if (_settings == null)
                _settings = SteamToolsSettings.GetOrCreate();

            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Settings"))
                SettingsService.OpenProjectSettings("Project/Steamworks");
            if (EditorGUILayout.LinkButton("Portal"))
            {
                if (_settings != null && _settings.ActiveApp.HasValue)
                    Application.OpenURL("https://partner.steamgames.com/apps/landing/" + _settings.ActiveApp.Value);
            }
            if (EditorGUILayout.LinkButton("Guide"))
                Application.OpenURL("https://kb.heathen.group/steam/features/leaderboards");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (_options == null)
                RefreshOptions();

            _selectedIndex = EditorGUILayout.Popup("Leaderboard", _selectedIndex, _options);

            if (_selectedIndex >= 0 && _options != null && _selectedIndex < _options.Length - 1)
            {
                _apiNameProp.stringValue = _options[_selectedIndex];
            }
            else
            {
                EditorGUILayout.PropertyField(_apiNameProp, new GUIContent("API Name"));
                EditorGUILayout.PropertyField(_createIfMissingProp);
                EditorGUILayout.PropertyField(_createAsDisplayProp);
                EditorGUILayout.PropertyField(_createWithSortProp);
            }

            EditorGUILayout.Space();

            HideAllAllowedComponents();
            DrawAddFieldDropdown();

            EditorGUI.indentLevel++;
            DrawModularComponents();
            EditorGUI.indentLevel--;

            DrawFunctionFlags();

            DrawFields<SettingsFieldAttribute>("Settings");
            DrawFields<ElementFieldAttribute>("Elements");
            DrawFields<TemplateFieldAttribute>("Templates");
            DrawEventFields();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
