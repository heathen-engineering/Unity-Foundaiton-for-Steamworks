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
    [CustomEditor(typeof(SteamStatData), true)]
    public class SteamStatDataEditor : ModularEditor
    {
        private string[] _options;
        private int _selectedIndex;
        private SerializedProperty _apiNameProp;
        private SteamToolsSettings _settings;

        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamStatIntDisplay),
            typeof(SteamStatFloatDisplay),
        };

        private void OnEnable()
        {
            _apiNameProp = serializedObject.FindProperty("apiName");
            RefreshOptions();
        }

        private void RefreshOptions()
        {
            _settings = SteamToolsSettings.GetOrCreate();
            var list = _settings != null && _settings.stats != null
                ? _settings.stats
                : new List<string>();

            if (list.Count > 0)
            {
                _options = list.ToArray();
                var current = _apiNameProp.stringValue;
                _selectedIndex = Mathf.Max(0, Array.IndexOf(_options, current));
            }
            else
            {
                _options = null;
            }
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
                Application.OpenURL("https://kb.heathen.group/steam/features/stats");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (_options == null || _options.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No stats found!\n\n" +
                    "Open Project Settings > Steamworks to configure your stats.",
                    MessageType.Warning
                );
                serializedObject.ApplyModifiedProperties();
                return;
            }

            _selectedIndex = EditorGUILayout.Popup("Stat", _selectedIndex, _options);
            if (_selectedIndex >= 0 && _selectedIndex < _options.Length)
                _apiNameProp.stringValue = _options[_selectedIndex];

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
