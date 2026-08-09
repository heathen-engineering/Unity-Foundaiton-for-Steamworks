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

#if !DISABLESTEAMWORKS && STEAM_INSTALLED && HEATHEN_GAMEFRAMEWORK
using System;
using System.Collections.Generic;
using System.Reflection;
using Heathen.SteamworksIntegration.API;

namespace Heathen.SteamworksIntegration
{
    /// <summary>
    /// Global Game Framework subsystem that owns the Steam API lifecycle. It pumps
    /// <c>SteamAPI.RunCallbacks()</c> every frame (<see cref="IOnUpdate"/>) while Steam is initialised —
    /// replacing the legacy background-thread pump (frame-capped, so no self-throttle, and it naturally
    /// pauses with the app on focus loss) — and shuts Steam down on framework teardown.
    ///
    /// <para><b>Start mode</b> is read from the generated <c>SteamTools.Game</c> wrapper (a value the
    /// developer sets in Project Settings ▸ Subsystems ▸ Steamworks and bakes via Generate):</para>
    /// <list type="bullet">
    /// <item><see cref="SubsystemStartMode.Automatic"/> — initialise Steam at boot.</item>
    /// <item><see cref="SubsystemStartMode.OnDemand"/> (default) — created and pumping, but Steam is only
    /// initialised when something calls <see cref="SteamTools.Interface.Initialise"/> (the
    /// <c>InitializeSteamworks</c> component, or your own code). This preserves the historic behaviour.</item>
    /// <item><see cref="SubsystemStartMode.Disabled"/> — this subsystem is not created and the generated
    /// wrapper refuses to initialise, so Steam never starts (ship the assemblies but stay dormant).</item>
    /// </list>
    /// </summary>
    [Subsystem(SubsystemScope.Global)]
    public sealed class SteamworksSubsystem : Subsystem, IOnUpdate, ISubsystemDebug
    {
        private SubsystemStartMode? _startMode;
        private bool _autoInitPending;

        /// <summary>
        /// The developer-selected start mode, resolved (and cached) from the generated wrapper. Defaults to
        /// <see cref="SubsystemStartMode.Automatic"/> when no wrapper / baked value is present, so Steam
        /// initialises with no component or code. A project that wants on-demand or disabled behaviour sets it
        /// in Project Settings ▸ Subsystems ▸ Steamworks and regenerates the wrapper.
        /// </summary>
        public override SubsystemStartMode StartMode => _startMode ??= ResolveStartMode();

        protected override void Initialize()
        {
            // Automatic: defer the actual init to the first update tick (see OnUpdate). We must NOT initialise
            // here: Initialize() runs at framework boot (SubsystemRegistration), but the Toolkit registers its
            // lifecycle hooks — including the Steam Input init handler — at BeforeSceneLoad, which is later.
            // Initialising now would run before those hooks exist, so Steam Input would never initialise and the
            // generated HandleInitialised would fail before raising OnReady. The first OnUpdate runs in the
            // player loop's Update phase, well after BeforeSceneLoad, so every hook is in place by then.
            // OnDemand: wait for an explicit Initialise(). Disabled never reaches here (ShouldCreate() is false).
            _autoInitPending = StartMode == SubsystemStartMode.Automatic;
        }

        protected override void Deinitialize()
        {
            // Idempotent with the Application.quitting handler — whichever runs first wins.
            App.Shutdown();
        }

        public void OnUpdate(float deltaTime)
        {
            // Automatic init, deferred from Initialize() so the Toolkit's BeforeSceneLoad hooks (Steam Input
            // init, per-tick handlers) are registered first. Runs once, on the first tick.
            if (_autoInitPending)
            {
                _autoInitPending = false;
                SteamTools.Interface.Initialise();
            }

            if (App.Initialised)
                App.PumpCallbacks();
        }

        public IEnumerable<(string label, string value)> GetDebugInfo()
        {
            yield return ("Start mode", StartMode.ToString());
            yield return ("Initialised", App.Initialised ? "yes" : "no");
            if (App.Initialised)
                yield return ("App ID", App.Id.ToString());
            if (App.HasInitialisationError)
                yield return ("Init error", App.InitialisationErrorMessage);
        }

        // The generated SteamTools.Game wrapper (in the project's own assembly) may expose a static
        // `StartMode` baked from settings. Read it reflectively so older wrappers without it default to
        // OnDemand and so the read does not depend on any [RuntimeInitializeOnLoadMethod] ordering.
        private static SubsystemStartMode ResolveStartMode()
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var gameType = asm.GetType("SteamTools.Game");
                    if (gameType == null) continue;

                    var prop = gameType.GetProperty("StartMode", BindingFlags.Public | BindingFlags.Static);
                    if (prop != null && prop.PropertyType == typeof(SubsystemStartMode))
                        return (SubsystemStartMode)prop.GetValue(null);

                    var field = gameType.GetField("StartMode", BindingFlags.Public | BindingFlags.Static);
                    if (field != null && field.FieldType == typeof(SubsystemStartMode))
                        return (SubsystemStartMode)field.GetValue(null);

                    break; // found the type but no StartMode member — use the default
                }
            }
            catch
            {
                // Fall through to the default.
            }

            return SubsystemStartMode.Automatic;
        }
    }
}
#endif
