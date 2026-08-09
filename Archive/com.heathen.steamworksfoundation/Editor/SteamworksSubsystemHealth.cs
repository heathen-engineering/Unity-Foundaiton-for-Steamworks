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

// Needs the Game Framework (this is the ISubsystemHealth contract's home) and the Steamworks subsystem type.
#if UNITY_EDITOR && !DISABLESTEAMWORKS && STEAM_INSTALLED && HEATHEN_GAMEFRAMEWORK
using Heathen.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Heathen.SteamworksIntegration.Editors
{
    /// <summary>
    /// Reports Steamworks setup problems to the Game Framework so they surface as badges on
    /// <c>Project ▸ Subsystems</c>, in the play-mode guard, and on the Scene-view attention overlay: the generated
    /// wrapper being out of date, and no App ID being set for the active app.
    /// </summary>
    public sealed class SteamworksSubsystemHealth : ISubsystemHealth
    {
        public Type SubsystemType => typeof(SteamworksSubsystem);

        public IEnumerable<SubsystemIssue> GetIssues()
        {
            var settings = SteamToolsSettings.GetOrCreate();

            // Reuse the registered "Steamworks" generator so the correct (Toolkit or Foundation) staleness check
            // and regenerate are used — never a mismatched one. The play-mode guard also handles stale source
            // generically, so this staleness badge is mainly for the overview and overlay.
            var generator = SettingsGenerators.All.FirstOrDefault(g => g.Name == "Steamworks");
            if (generator != null && generator.IsStale())
                yield return new SubsystemIssue(
                    SubsystemHealthSeverity.Warning,
                    "The generated Steam wrapper is out of date. Build to apply your latest settings.",
                    "Build",
                    () => { generator.Generate(); AssetDatabase.Refresh(); });

            if (!settings.ActiveApp.HasValue || settings.ActiveApp.Value == 0)
                yield return new SubsystemIssue(
                    SubsystemHealthSeverity.Warning,
                    "No App ID is set for the active app. Steam cannot initialise without one.",
                    "Open Settings",
                    () => SettingsService.OpenProjectSettings("Project/Subsystems/Steamworks"));
        }
    }
}
#endif
