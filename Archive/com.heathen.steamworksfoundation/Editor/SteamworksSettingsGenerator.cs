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

// Only when the Game Framework is present (the shared generator pipeline lives there) AND the Toolkit is NOT
// installed. When the Toolkit is present it supplies the richer generator via its own ISettingsGenerator, and
// this one is compiled out so exactly one generator is discovered for Steamworks.
#if UNITY_EDITOR && !DISABLESTEAMWORKS && STEAM_INSTALLED && HEATHEN_GAMEFRAMEWORK && !HEATHEN_TOOLKIT_INSTALLED
using Heathen.Editor;

namespace Heathen.SteamworksIntegration.Editors
{
    /// <summary>
    /// Plugs the Foundation Steam code generator into the Game Framework's shared generation pipeline so the
    /// framework play-mode guard and build hook treat Steamworks like every other subsystem. Emits C# source, so
    /// it is <see cref="GeneratorOutput.SourceCode"/> — never regenerated mid-transition (prompt-only), which is
    /// why editing Steam settings does not trigger constant recompiles.
    /// </summary>
    public sealed class SteamworksSettingsGenerator : ISettingsGenerator
    {
        public string Name => "Steamworks";

        public GeneratorOutput Output => GeneratorOutput.SourceCode;

        public bool IsStale() => SteamToolsCodeGenerator.IsStale(SteamToolsSettings.GetOrCreate());

        public void Generate() => SteamToolsCodeGenerator.Generate(SteamToolsSettings.GetOrCreate());
    }
}
#endif
