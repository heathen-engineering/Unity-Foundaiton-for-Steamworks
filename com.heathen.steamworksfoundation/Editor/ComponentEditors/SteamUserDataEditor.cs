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

#if UNITY_EDITOR && !DISABLESTEAMWORKS && STEAM_INSTALLED && !HEATHEN_TOOLKIT_INSTALLED
using System;
using UnityEditor;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [CustomEditor(typeof(SteamUserData), true)]
    public class SteamUserDataEditor : ModularEditor
    {
        private SerializedProperty _localUserProp;
        private SteamToolsSettings _settings;

        /// <summary>
        /// Allowed child component types for the User modular editor.
        /// SteamUserAvatar is Toolkit-only and is not listed here.
        /// </summary>
        protected override Type[] AllowedTypes => new Type[]
        {
            typeof(SteamUserName),
            typeof(SteamUserLevel),
            typeof(SteamUserStatus),
            typeof(SteamUserDataEvents),
            typeof(SteamUserHexInput),
            typeof(SteamUserHexLabel),
        };

        private void OnEnable()
        {
            _settings = SteamToolsSettings.GetOrCreate();
            _localUserProp = serializedObject.FindProperty("localUser");
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
                Application.OpenURL("https://kb.heathen.group/steam/features/users");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_localUserProp);

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
