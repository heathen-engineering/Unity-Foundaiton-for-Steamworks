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

#if UNITY_EDITOR && !DISABLESTEAMWORKS && STEAM_INSTALLED && HEATHEN_GAMEFRAMEWORK
using System;
using Heathen.Editor;
using UnityEditor;

namespace Heathen.SteamworksIntegration.Editors
{
    /// <summary>
    /// Surfaces the Steamworks subsystem's start mode to the framework's <c>Project ▸ Subsystems</c> page — the
    /// one standard place to choose Disabled / OnDemand / Automatic. The value is stored in
    /// <see cref="SteamToolsSettings"/> and baked into the generated <c>SteamTools.Game</c> wrapper, so a
    /// regenerate is required for a change to take effect at runtime. Also links the subsystem's overview header
    /// to the Steamworks settings page.
    /// </summary>
    public sealed class SteamworksSubsystemConfigEditor : ISubsystemConfigEditor, ISubsystemSettingsPage, ISubsystemDocumentation
    {
        public Type SubsystemType => typeof(SteamworksSubsystem);

        public void Open() => SettingsService.OpenProjectSettings("Project/Subsystems/Steamworks");
        public string DocumentationUrl => "https://heathen.group/kb/steam-welcome/";

        public SubsystemStartMode StartMode
        {
            get => SteamToolsSettings.GetOrCreate().startMode;
            set
            {
                var settings = SteamToolsSettings.GetOrCreate();
                if (settings.startMode == value) return;
                settings.startMode = value;
                SteamToolsSettings.Save();
            }
        }

        public string ApplyHint =>
            "Baked into the generated wrapper — run Generate Code on the Steamworks page for this to take effect at runtime.";
    }
}
#endif
