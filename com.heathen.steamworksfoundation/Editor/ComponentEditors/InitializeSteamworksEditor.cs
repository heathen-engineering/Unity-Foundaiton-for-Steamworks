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
using UnityEditor;
using UnityEngine;

namespace Heathen.SteamworksIntegration
{
    [CustomEditor(typeof(InitializeSteamworks), true)]
    public class InitializeSteamworksEditor : Editor
    {
        private SteamToolsSettings _settings;

        private void OnEnable()
        {
            _settings = SteamToolsSettings.GetOrCreate();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.LinkButton("Settings"))
                SettingsService.OpenProjectSettings("Project/Steamworks");
            if (_settings != null && _settings.ActiveApp.HasValue)
            {
                if (EditorGUILayout.LinkButton("Portal"))
                    Application.OpenURL("https://partner.steamgames.com/apps/landing/" + _settings.ActiveApp.Value);
            }
            if (EditorGUILayout.LinkButton("Guide"))
                Application.OpenURL("https://kb.heathen.group/steam");
            if (EditorGUILayout.LinkButton("Support"))
                Application.OpenURL("https://discord.gg/heathen-group-463483739612381204");
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
